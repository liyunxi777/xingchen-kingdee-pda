# xingchen-kingdee-pda

星辰 PDA 端金蝶云星空（Kingdee Cloud）API 数据查询与入库 WinForms 客户端。

## 功能

- 通过金蝶开放平台 API 查询物料、工单、仓库、部门、员工、客户、计量单位、Space、销售订单、入库/出库等数据。
- 单窗口界面：左侧 `TabControl`（物料 / 工单），右侧结果表格 + 多按钮查询区。
- 5 分钟业务 Token 缓存：同一按钮 5 分钟内重复点击回放缓存，过期自动刷新当前显示。
- 左侧 Tab 数据独立 5 分钟轮询，哈希比对有变化才刷新表格。
- 金蝶 app-token 约 24 小时有效，程序在满 20 小时时自动续期。
- 通用入库模型：`api_record` / `api_record_field` / `api_record_change` / `api_sync_log`。
- 请求审计表 `api_request_log`：记录设备编号、请求时间、按钮/接口、请求参数、返回条数与状态。
- 详情类按钮自动从列表选中行（或第一条数据）智能取内码，不再写死单号。

## 项目结构

```
WindowsApplication3.sln
├── LrGetToken/                  # 金蝶 API 底层类库（C#）
│   ├── APIClient.cs             # HTTP 请求、签名、GetResponse()
│   ├── LrGetMtrlDatum.cs        # 各接口取数封装 + RequestObserver 请求审计回调
│   ├── clsGetToken.cs           # appSecret / app-token 获取
│   └── SHAHelper.cs             # HMAC-SHA256 签名
└── WindowsApplication3/         # WinForms 主程序
    ├── FrmTest.cs               # 主窗体（当前功能版本，C#）
    ├── FrmTest.Designer.cs      # 单窗口 SplitContainer 布局
    ├── Data/                    # EF Core 实体、DbContext、入库与日志写入
    ├── App.config               # 数据库连接串、DeviceId
    ├── KdXingCheng.csproj       # 推荐编译：C# 项目，包含全部新功能
    └── KdXingCheng.vbproj       # 旧版 VB 项目（FrmTest.vb），仅保留兼容
```

> 注意：目录下同时存在 `FrmTest.cs` 与 `FrmTest.vb`。**请编译 `KdXingCheng.csproj`**；
> `KdXingCheng.vbproj` 是旧代码，不包含 Token 缓存、Tab 自动刷新、请求日志与数据库入库功能。

## 环境要求

- .NET Framework 4.8
- Visual Studio 2019 / 2022（Windows Forms 开发负载）
- SQL Server（本地或可连接实例）
- NuGet：`Newtonsoft.Json 13.0.3`、`Microsoft.EntityFrameworkCore 3.1.32`、`Microsoft.EntityFrameworkCore.SqlServer 3.1.32`

## 配置

1. 修改 `WindowsApplication3/App.config`：

```xml
<connectionStrings>
  <add name="XingChenDb"
       connectionString="Server=localhost;Database=XingChenDb;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="Microsoft.Data.SqlClient"/>
</connectionStrings>
<appSettings>
  <add key="DeviceId" value="PDA-01"/>
</appSettings>
```

每台 PDA 填写各自的 `DeviceId`；留空则自动使用机器名。

2. 金蝶凭证目前写在 `FrmTest.cs` 顶部字段中：

```csharp
private string AppKey = "...";
private string clientId = "...";
private string clientSecret = "...";
private string outerInstanceId = "...";
```

**安全提示**：`clientSecret` 属于敏感信息。当前仓库为公开仓库，而源码中仍存在硬编码凭证，
请尽快把凭证改为从 `App.config`、环境变量或服务端下发读取，并在金蝶后台轮换该 `clientSecret`。

3. 数据库表结构见 `schema.sql`。程序启动时会确保 `api_request_log` 表存在，兼容老库。

## 构建

用 Visual Studio 打开 `WindowsApplication3.sln`，生成 `KdXingCheng.csproj`（Debug | AnyCPU）。

## 修改记录

详见 `docs/change-log/`。
