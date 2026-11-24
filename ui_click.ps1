Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, WindowsBase
$ErrorActionPreference = 'Continue'
$root = [System.Windows.Automation.AutomationElement]::RootElement
$cond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, '*星辰API调用*')
$win = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
if (-not $win) { Write-Host 'Window NOT found'; exit 1 }
Write-Host ('Found: ' + $win.Current.Name)
$btnName = $args[0]
if (-not $btnName) { $btnName = '获取物料' }
$nameCond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $btnName)
$btn = $win.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $nameCond)
if ($btn) {
    Write-Host ('Found button: ' + $btn.Current.Name + ' - Class: ' + $btn.Current.ClassName)
    $vp = $btn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $vp.Invoke()
    Write-Host ('Invoked ' + $btnName)
    Start-Sleep -Seconds 3
    $win2 = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
    if ($win2) { Write-Host ('New Title: ' + $win2.Current.Name) }
} else { Write-Host ('Button NOT found: ' + $btnName) }
