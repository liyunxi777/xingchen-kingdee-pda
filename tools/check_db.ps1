# Check XingChen API data in SQL Server (created by EF Core)
param(
    [string]$Server = "localhost",
    [string]$DbName = "XingChenDb",
    [switch]$ShowRows
)

$connStr = "Server=$Server;Database=$DbName;Integrated Security=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
Write-Host ("Connected to SQL Server " + $Server + "  version: " + $conn.ServerVersion)

function RunQ($title, $sql) {
    Write-Host ""
    Write-Host ("=== " + $title + " ===")
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $reader = $cmd.ExecuteReader()
    $table = New-Object System.Data.DataTable
    $table.Load($reader)
    if ($table.Rows.Count -eq 0) { Write-Host "(empty)" }
    else { $table | Format-Table -AutoSize | Out-String -Width 220 | Write-Host }
}

RunQ "tables" "SELECT name AS table_name FROM sys.tables WHERE type='U' ORDER BY name"
RunQ "columns (EF Core created)" "SELECT OBJECT_NAME(object_id) AS tbl, name AS col, type_name(system_type_id) AS type, max_length FROM sys.columns WHERE OBJECT_NAME(object_id) LIKE 'api%' ORDER BY object_id, column_id"
RunQ "row counts" @"
SELECT 'api_record' AS tbl, COUNT(*) AS cnt FROM api_record
UNION ALL SELECT 'api_record_field', COUNT(*) FROM api_record_field
UNION ALL SELECT 'api_record_change', COUNT(*) FROM api_record_change
UNION ALL SELECT 'api_sync_log', COUNT(*) FROM api_sync_log
"@
RunQ "per endpoint (same data stored once, SyncCount = times seen)" @"
SELECT Endpoint, EndpointName, COUNT(*) AS records, SUM(SyncCount) AS seen_times, SUM(ChangeCount) AS changed
FROM api_record GROUP BY Endpoint, EndpointName ORDER BY records DESC
"@
RunQ "latest sync logs" "SELECT TOP 8 Id, Endpoint, SyncTime, TotalRows, InsertedRows, UpdatedRows, UnchangedRows, Status, DurationMs FROM api_sync_log ORDER BY Id DESC"

if ($ShowRows) {
    RunQ "latest 10 records" "SELECT TOP 10 Id, Endpoint, BizKey, Title, FieldCount, SyncCount, ChangeCount, LastSyncTime FROM api_record ORDER BY Id DESC"
    RunQ "flattened dynamic fields sample" "SELECT TOP 20 RecordId, FieldName, FieldValue, FieldType FROM api_record_field ORDER BY Id DESC"
    RunQ "change history (field diff)" "SELECT TOP 10 Id, Endpoint, BizKey, OldHash, NewHash, ChangedCount, ChangeTime FROM api_record_change ORDER BY Id DESC"
}

$conn.Close()
