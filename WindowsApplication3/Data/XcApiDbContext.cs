using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 星辰API数据上下文（EF Core + SQL Server）。
    /// 连接字符串读取 App.config 中的 connectionStrings/XingChenDb。
    /// </summary>
    public class XcApiDbContext : DbContext
    {
        public const string ConnectionStringName = "XingChenDb";

        public virtual DbSet<ApiRecord> ApiRecords { get; set; }
        public virtual DbSet<ApiRecordField> ApiRecordFields { get; set; }
        public virtual DbSet<ApiRecordChange> ApiRecordChanges { get; set; }
        public virtual DbSet<ApiSyncLog> ApiSyncLogs { get; set; }
        public virtual DbSet<ApiRequestLog> ApiRequestLogs { get; set; }

        public static string GetConnectionString()
        {
            string cs = null;
            try
            {
                ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                if (setting != null) cs = setting.ConnectionString;
            }
            catch
            {
                cs = null;
            }
            if (string.IsNullOrEmpty(cs))
            {
                cs = "Server=localhost;Database=XingChenDb;Integrated Security=True;MultipleActiveResultSets=True";
            }
            return cs;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApiRecord>(entity =>
            {
                entity.ToTable("api_record");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Endpoint).HasMaxLength(200).IsRequired();
                entity.Property(e => e.EndpointName).HasMaxLength(100);
                entity.Property(e => e.BizKey).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(300);
                entity.Property(e => e.DataHash).HasMaxLength(64).IsRequired();
                entity.Property(e => e.Json).HasColumnType("nvarchar(max)");
                entity.Property(e => e.FirstSyncTime).IsRequired();
                entity.Property(e => e.LastSyncTime).IsRequired();
                // 同一接口 + 同一业务键 只允许一条记录 => 相同数据只存一次
                entity.HasIndex(e => new { e.Endpoint, e.BizKey })
                      .IsUnique()
                      .HasName("ux_api_record_endpoint_bizkey");
                entity.HasIndex(e => e.DataHash).HasName("ix_api_record_datahash");
            });

            modelBuilder.Entity<ApiRecordField>(entity =>
            {
                entity.ToTable("api_record_field");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FieldName).HasMaxLength(300).IsRequired();
                entity.Property(e => e.FieldValue).HasColumnType("nvarchar(max)");
                entity.Property(e => e.FieldType).HasMaxLength(20);
                entity.HasIndex(e => new { e.RecordId, e.FieldName })
                      .IsUnique()
                      .HasName("ux_api_record_field_record_field");
                entity.HasOne(e => e.Record)
                      .WithMany(r => r.Fields)
                      .HasForeignKey(e => e.RecordId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApiRecordChange>(entity =>
            {
                entity.ToTable("api_record_change");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Endpoint).HasMaxLength(200).IsRequired();
                entity.Property(e => e.EndpointName).HasMaxLength(100);
                entity.Property(e => e.BizKey).HasMaxLength(200).IsRequired();
                entity.Property(e => e.OldHash).HasMaxLength(64);
                entity.Property(e => e.NewHash).HasMaxLength(64);
                entity.Property(e => e.ChangedFields).HasColumnType("nvarchar(max)");
                entity.Property(e => e.OldJson).HasColumnType("nvarchar(max)");
                entity.Property(e => e.NewJson).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ChangeTime).IsRequired();
                entity.HasIndex(e => new { e.Endpoint, e.BizKey, e.ChangeTime })
                      .HasName("ix_api_record_change_endpoint_bizkey_time");
            });

            modelBuilder.Entity<ApiSyncLog>(entity =>
            {
                entity.ToTable("api_sync_log");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Endpoint).HasMaxLength(200);
                entity.Property(e => e.EndpointName).HasMaxLength(100);
                entity.Property(e => e.SyncTime).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Message).HasColumnType("nvarchar(max)");
                entity.HasIndex(e => e.SyncTime).HasName("ix_api_sync_log_synctime");
            });

            modelBuilder.Entity<ApiRequestLog>(entity =>
            {
                entity.ToTable("api_request_log");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DeviceId).HasMaxLength(200);
                entity.Property(e => e.Endpoint).HasMaxLength(200);
                entity.Property(e => e.EndpointName).HasMaxLength(100);
                entity.Property(e => e.RequestParams).HasColumnType("nvarchar(max)");
                entity.Property(e => e.RequestTime).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Message).HasColumnType("nvarchar(max)");
                entity.HasIndex(e => e.RequestTime).HasName("ix_api_request_log_time");
                entity.HasIndex(e => e.DeviceId).HasName("ix_api_request_log_device");
            });
        }

        /// <summary>
        /// 确保数据库与各表已存在（不存在则自动创建）。
        /// </summary>
        public static string EnsureDatabase(out bool ok)
        {
            try
            {
                using (XcApiDbContext db = new XcApiDbContext())
                {
                    bool created = db.Database.EnsureCreated();
                    // EnsureCreated 对"已存在的库"不会补建新增表，这里幂等补建 api_request_log，
                    // 这样老库无需删除重建就能记录请求日志。失败不影响主库状态。
                    EnsureRequestLogTable(db);
                    ok = true;
                    return created
                        ? "数据库已自动创建：" + DatabaseLabel()
                        : "数据库连接正常（已存在）：" + DatabaseLabel();
                }
            }
            catch (Exception ex)
            {
                ok = false;
                return "数据库连接失败：" + ex.Message;
            }
        }

        /// <summary>
        /// 幂等补建请求日志表：已存在的老库通过 EnsureCreated 不会自动建新表，
        /// 用 CREATE TABLE IF NOT EXISTS 语义（SQL Server 用 OBJECT_ID 判断）保证不报错、不丢数据。
        /// 异常在此吞掉，绝不影响主库连接判定。
        /// </summary>
        private static void EnsureRequestLogTable(XcApiDbContext db)
        {
            try
            {
                db.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.api_request_log', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[api_request_log](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_api_request_log] PRIMARY KEY CLUSTERED,
        [DeviceId] NVARCHAR(200) NULL,
        [Endpoint] NVARCHAR(200) NULL,
        [EndpointName] NVARCHAR(100) NULL,
        [RequestParams] NVARCHAR(MAX) NULL,
        [RequestTime] DATETIME2 NOT NULL,
        [ResponseRows] INT NOT NULL DEFAULT(0),
        [Status] NVARCHAR(20) NULL,
        [Message] NVARCHAR(MAX) NULL
    );
    CREATE INDEX [ix_api_request_log_time] ON [dbo].[api_request_log]([RequestTime]);
    CREATE INDEX [ix_api_request_log_device] ON [dbo].[api_request_log]([DeviceId]);
END
");
            }
            catch
            {
                // 建表失败不影响主库；请求日志写入时也会再次静默失败，不会崩溃
            }
        }

        public static string DatabaseLabel()
        {
            return "Server=localhost; Database=XingChenDb; Windows身份验证";
        }
    }
}
