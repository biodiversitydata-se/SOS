param($SiteName)

Write-Verbose -Verbose "Entering script StartSite.ps1"

Start-Website -Name "$SiteName"

Write-Verbose -Verbose "Leaving script StartSite.ps1"