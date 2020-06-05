param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('check', 'kill')]
    [string] $Operation = 'check'
)

$processName = 'nginx'

$processes = Get-Process $processName -ErrorAction SilentlyContinue

if ($Operation -eq 'kill') {
    if ($null -ne $processes) {
        $processes | Stop-Process
    }
}

$processes