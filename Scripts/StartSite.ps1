param($SiteName, $AppPoolName)

Write-Verbose -Verbose "Entering script StartSite.ps1"

Start-WebAppPool -Name "$AppPoolName" 
Start-Website -Name "$SiteName"

Write-Verbose -Verbose "Leaving script StartSite.ps1"