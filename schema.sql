-- ============================================================
-- Kingdee 物料数据 - 数据库建表脚本
-- 支持: SQL Server / MySQL / PostgreSQL (注释掉不适用的部分即可)
-- ============================================================

-- 1. 物料主表 (materials)
CREATE TABLE materials (
    material_id          VARCHAR(32)    PRIMARY KEY,              -- 物料ID（金蝶内部ID）
    material_number      VARCHAR(100)   NOT NULL,                 -- 物料编码
    material_name        VARCHAR(200)   NOT NULL,                 -- 物料名称
    material_full_name   VARCHAR(500),                            -- 物料全名
    specification        VARCHAR(500),                            -- 规格
    model                VARCHAR(200),                            -- 型号
    barcode              VARCHAR(200),                            -- 条码
    material_group_id    VARCHAR(32),                             -- 物料分类ID
    material_group_name  VARCHAR(100),                            -- 物料分类名称
    material_group_number VARCHAR(50),                            -- 物料分类编码
    measure_unit_id      VARCHAR(32),                             -- 计量单位ID
    measure_unit_name    VARCHAR(50),                             -- 计量单位名称
    is_multi_unit        TINYINT       DEFAULT 0,                 -- 是否多单位 0=否 1=是
    aux_prop_id          VARCHAR(32),                             -- 辅助属性ID
    aux_prop_name        VARCHAR(200),                            -- 辅助属性名称
    enable               TINYINT       DEFAULT 1,                 -- 是否启用 0=禁用 1=启用
    remark               VARCHAR(1000),                           -- 备注
    create_time          DATETIME,                                -- 创建时间
    modify_time          DATETIME,                                -- 修改时间
    -- JSON 字段（MySQL 用 JSON, PG 用 JSONB, SQL Server 用 NVARCHAR(MAX)）
    labels_json          NVARCHAR(MAX),                           -- 标签 JSON (mul_label)
    custom_field_json    NVARCHAR(MAX),                           -- 自定义字段 JSON
    units_json           NVARCHAR(MAX),                           -- 单位列表 JSON
    sync_time            DATETIME      DEFAULT GETDATE(),         -- 同步时间
    UNIQUE INDEX idx_material_number (material_number)
);

-- MySQL 版本:
-- CREATE TABLE materials (
--     material_id          VARCHAR(32)    PRIMARY KEY,
--     material_number      VARCHAR(100)   NOT NULL UNIQUE,
--     material_name        VARCHAR(200)   NOT NULL,
--     material_full_name   VARCHAR(500),
--     specification        VARCHAR(500),
--     model                VARCHAR(200),
--     barcode              VARCHAR(200),
--     material_group_id    VARCHAR(32),
--     material_group_name  VARCHAR(100),
--     material_group_number VARCHAR(50),
--     measure_unit_id      VARCHAR(32),
--     measure_unit_name    VARCHAR(50),
--     is_multi_unit        TINYINT       DEFAULT 0,
--     aux_prop_id          VARCHAR(32),
--     aux_prop_name        VARCHAR(200),
--     enable               TINYINT       DEFAULT 1,
--     remark               TEXT,
--     create_time          DATETIME,
--     modify_time          DATETIME,
--     labels_json          JSON,
--     custom_field_json    JSON,
--     units_json           JSON,
--     sync_time            DATETIME      DEFAULT CURRENT_TIMESTAMP
-- );

-- PostgreSQL 版本:
-- CREATE TABLE materials (
--     material_id          VARCHAR(32)    PRIMARY KEY,
--     material_number      VARCHAR(100)   NOT NULL UNIQUE,
--     material_name        VARCHAR(200)   NOT NULL,
--     material_full_name   VARCHAR(500),
--     specification        VARCHAR(500),
--     model                VARCHAR(200),
--     barcode              VARCHAR(200),
--     material_group_id    VARCHAR(32),
--     material_group_name  VARCHAR(100),
--     material_group_number VARCHAR(50),
--     measure_unit_id      VARCHAR(32),
--     measure_unit_name    VARCHAR(50),
--     is_multi_unit        SMALLINT      DEFAULT 0,
--     aux_prop_id          VARCHAR(32),
--     aux_prop_name        VARCHAR(200),
--     enable               SMALLINT      DEFAULT 1,
--     remark               TEXT,
--     create_time          TIMESTAMP,
--     modify_time          TIMESTAMP,
--     labels_json          JSONB,
--     custom_field_json    JSONB,
--     units_json           JSONB,
--     sync_time            TIMESTAMP     DEFAULT NOW()
-- );


-- 2. 物料单位明细表 (material_units)
-- 一对多：一个物料可以有多个单位
CREATE TABLE material_units (
    id              INT           IDENTITY(1,1) PRIMARY KEY,
    material_id     VARCHAR(32)   NOT NULL,
    unit_id         VARCHAR(32)   NOT NULL,
    unit_name       VARCHAR(50),
    unit_type       TINYINT,              -- 1=基本单位 2=辅助单位
    unit_convert    DECIMAL(18,6),         -- 换算率
    FOREIGN KEY (material_id) REFERENCES materials(material_id) ON DELETE CASCADE
);


-- 3. 物料标签表 (material_labels)
-- 一对多：一个物料可以有多个标签
CREATE TABLE material_labels (
    id              INT           IDENTITY(1,1) PRIMARY KEY,
    material_id     VARCHAR(32)   NOT NULL,
    label_id        VARCHAR(32),
    label_name      VARCHAR(100),
    FOREIGN KEY (material_id) REFERENCES materials(material_id) ON DELETE CASCADE
);


-- 4. 同步日志表 (sync_log)
-- 记录每次数据同步的时间、数量和状态
CREATE TABLE sync_log (
    id              INT           IDENTITY(1,1) PRIMARY KEY,
    sync_time       DATETIME      DEFAULT GETDATE(),
    record_count    INT           DEFAULT 0,
    status          VARCHAR(20)   DEFAULT 'success',  -- success / failed
    error_msg       NVARCHAR(MAX),
    api_endpoint    VARCHAR(200)
);