param($SiteName)

Write-Verbose -Verbose "Entering script StopSite.ps1"

Stop-Website -Name "$SiteName"

Get-WmiObject -Class Win32_Process -Filter "Name='dotnet.exe'" | 
Where-Object { $_.GetOwner().User -eq "$SiteName" } | 
Foreach-Object { $_.Terminate() }

Write-Verbose -Verbose "Leaving script StopSite.ps1"