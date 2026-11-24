# Verify DynamicJsonStore (EF Core + SQL Server) behaviour:
#   1) first save   -> insert
#   2) same data    -> NOT stored twice (only SyncCount + 1)
#   3) changed data -> compare then update, write change history
# NOTE: keep this file ASCII-only, Windows PowerShell 5.1 reads it as ANSI.
# Usage: powershell -File tools\test_store.ps1 [-Cleanup]
param([switch]$Cleanup)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$bin = Join-Path $root 'WindowsApplication3\bin\Debug'
$exe = Join-Path $bin 'WindowsApplication3.exe'
if (-not (Test-Path $exe)) { throw ("build output not found: " + $exe) }

# make dependencies in bin\Debug resolvable from the PowerShell host
$binPath = $bin
[System.AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($s, $e)
    $n = ($e.Name -split ',')[0]
    $p = Join-Path $binPath ($n + '.dll')
    if (Test-Path $p) { return [System.Reflection.Assembly]::LoadFrom($p) }
    return $null
})

$connStr = "Server=localhost;Database=XingChenDb;Integrated Security=True;"

function RunNonQuery($sql) {
    $c = New-Object System.Data.SqlClient.SqlConnection $connStr
    $c.Open()
    $cmd = $c.CreateCommand()
    $cmd.CommandText = $sql
    $n = $cmd.ExecuteNonQuery()
    $c.Close()
    return $n
}

# clean previous test rows so the run is deterministic
RunNonQuery @"
DELETE FROM api_record_field WHERE RecordId IN (SELECT Id FROM api_record WHERE Endpoint='test/demo');
DELETE FROM api_record WHERE Endpoint='test/demo';
DELETE FROM api_record_change WHERE Endpoint='test/demo';
DELETE FROM api_sync_log WHERE Endpoint='test/demo';
"@ | Out-Null
Write-Host "previous test rows cleaned"

$asm = [System.Reflection.Assembly]::LoadFrom($exe)
$storeType = $asm.GetType('WindowsApplication3.Data.DynamicJsonStore')
$store = [System.Activator]::CreateInstance($storeType)
$save = $storeType.GetMethod('Save')

function InvokeSave($label, $json) {
    Write-Host ""
    Write-Host ("--- " + $label + " ---")
    $r = $save.Invoke($store, @('test/demo', 'StoreTest', $json))
    Write-Host ("Success=" + $r.Success + " Total=" + $r.Total + " Inserted=" + $r.Inserted +
                " Updated=" + $r.Updated + " Unchanged=" + $r.Unchanged + " | " + $r.Summary)
    return $r
}

# name = "Test Material A" / "Test Material B" written with \uXXXX so the file stays ASCII
$v1 = '{"code":"200","message":"success","data":{"rows":[{"id":"900001","number":"TEST-900001","name":"\u6d4b\u8bd5\u7269\u6599A","qty":10,"unit_id":"3"}]}}'
$v1dup = '{"data":{"rows":[{"unit_id":"3","id":"900001","name":"\u6d4b\u8bd5\u7269\u6599A","number":"TEST-900001","qty":10}]}}'
$v2 = '{"code":"200","message":"success","data":{"rows":[{"id":"900001","number":"TEST-900001","name":"\u6d4b\u8bd5\u7269\u6599B","qty":20,"unit_id":"3","remark":"changed"}]}}'

$r1 = InvokeSave '1) first save, expect Inserted=1' $v1
$r2 = InvokeSave '2) same content, different key order, expect Unchanged=1 (no duplicate row)' $v1dup
$r3 = InvokeSave '3) changed content, expect Updated=1 + change history' $v2

$check = @"
SELECT 'api_record' AS tbl, COUNT(*) AS cnt FROM api_record WHERE Endpoint='test/demo'
UNION ALL SELECT 'api_record_field', COUNT(*) FROM api_record_field WHERE RecordId IN (SELECT Id FROM api_record WHERE Endpoint='test/demo')
UNION ALL SELECT 'api_record_change', COUNT(*) FROM api_record_change WHERE Endpoint='test/demo'
"@
$c = New-Object System.Data.SqlClient.SqlConnection $connStr
$c.Open()
$cmd = $c.CreateCommand()
$cmd.CommandText = $check
$rd = $cmd.ExecuteReader()
$dt = New-Object System.Data.DataTable
$dt.Load($rd)
Write-Host ""
Write-Host "=== rows in SQL Server for endpoint test/demo ==="
$dt | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

$cmd.CommandText = "SELECT BizKey, Title, SyncCount, ChangeCount, FieldCount FROM api_record WHERE Endpoint='test/demo'"
$rd = $cmd.ExecuteReader()
$dt2 = New-Object System.Data.DataTable
$dt2.Load($rd)
$dt2 | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

$cmd.CommandText = "SELECT ChangedCount, ChangedFields FROM api_record_change WHERE Endpoint='test/demo' ORDER BY Id"
$rd = $cmd.ExecuteReader()
$dt3 = New-Object System.Data.DataTable
$dt3.Load($rd)
Write-Host "=== change history (field diff) ==="
$dt3 | Format-List | Out-String -Width 200 | Write-Host
$c.Close()

if ($Cleanup) {
    RunNonQuery @"
DELETE FROM api_record_field WHERE RecordId IN (SELECT Id FROM api_record WHERE Endpoint='test/demo');
DELETE FROM api_record WHERE Endpoint='test/demo';
DELETE FROM api_record_change WHERE Endpoint='test/demo';
DELETE FROM api_sync_log WHERE Endpoint='test/demo';
"@ | Out-Null
    Write-Host "test rows cleaned (-Cleanup)"
} else {
    Write-Host "test rows kept, run again with -Cleanup to remove them"
}
