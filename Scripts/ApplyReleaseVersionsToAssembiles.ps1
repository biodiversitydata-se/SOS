##-----------------------------------------------------------------------
## <copyright file="ApplyReleaseVersionToAssemblies.ps1">(c) Microsoft Corporation. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
##-----------------------------------------------------------------------
# Look for a release_0_0_0 pattern in the release number. 
# If found use it to ReleaseVersion the appsettings.json.
#
# For example, if the 'Release number format' build process parameter 
# $(Release.DefinitionName)_$(Date:yyyyMMdd)_$(rev:rr)_$(Release.ReleaseId) Note that BReleaseDefinitionName must ends with "Release"
# then your build numbers come out like this:
# "<Name>_20170308_02_244_1"
# This script would then apply version 20170308_02_244_1 to your assemblies.
# This regexp pattern must differ from the pattern in ApplyVersionToAssemblies.ps1 if not incorrect "parameter" appsettings.json in will be set.
# TFS artifactstaging directory must include folder "drop", it's below that folder structure appsettings.json is to be found.

# Enable -Verbose option
[CmdletBinding()]

# Regular expression pattern to find the version in the release number 
# and then apply it to the assemblies  $(Release.DefinitionName)_$(Date:yyyyMMdd)_$(rev:rr)_$(Release.ReleaseId)
$VersionRegex = "[R|r][e][l][e][a][s][e].\d+.\d+.\d+"

Write-Verbose -Verbose "Entering script ApplyReleaseVersionToAssemblies.ps1"

# If this script is not running on a build server, remind user to 
# set environment variables so that this script can be debugged
# RELEASE_RELEASENAME: Release.<Name>_<Date>_.02_244_1
if(-not ($Env:AGENT_RELEASEDIRECTORY -and $Env:RELEASE_RELEASENAME))
{
    Write-Error "You must set the following environment variables"
    Write-Error "to test this script interactively."
    Write-Host '$Env:AGENT_RELEASEDIRECTORY - For example, enter something like:'
    Write-Host '$Env:AGENT_RELEASEDIRECTORY = "E:\agent1\_work\c0623ff22"'
    Write-Host '$Env:BUILD_BUILDNUMBER - For example, enter something like:'
    Write-Host '$Env:BUILD_BUILDNUMBER = "<Name>Release_000000_00_0000"'
    exit 1
}

# Make sure path to source code directory is available
if (-not $Env:AGENT_RELEASEDIRECTORY)
{
    Write-Error ("AGENT_RELEASEDIRECTORY environment variable is missing.")
    exit 1
}
elseif (-not (Test-Path $Env:AGENT_RELEASEDIRECTORY))
{
    Write-Error "BUILD_SOURCESDIRECTORY does not exist: $Env:AGENT_RELEASEDIRECTORY"
    exit 1
}
Write-Verbose -Verbose "BUILD_SOURCESDIRECTORY: $Env:AGENT_RELEASEDIRECTORY"

# Make sure there is a build number
if (-not $Env:RELEASE_RELEASENAME)
{
    Write-Error ("RELEASE_RELEASENAME environment variable is missing.")
    exit 1
}
Write-Verbose -Verbose "RELEASE_RELEASENAME: $Env:RELEASE_RELEASENAME"

# Get and validate the version data
$VersionData = [regex]::matches($Env:RELEASE_RELEASENAME,$VersionRegex)
switch($VersionData.Count)
{
   0        
      { 
         Write-Error "Could not find version number data in RELEASE_RELEASENAME."
         exit 1
      }
   1 {}
   default 
      { 
         Write-Warning "Found more than instance of version data in RELEASE_RELEASENAME." 
         Write-Warning "Will assume first instance is version."
      }
}
$NewVersion = $VersionData[0]
Write-Verbose -Verbose "Revision version: $NewVersion"

$BuildDate = ($NewVersion -split '_')[1]
Write-Verbose -Verbose "Revision date: $BuildDate"

$Revision = ($NewVersion -split '_')[2]
Write-Verbose -Verbose "Revision revision: $Revision"

$RevisionId = ($NewVersion -split '_')[3]
Write-Verbose -Verbose "Revision id: $RevisionId"

# Apply the version to the assembly property files
$files = gci $Env:AGENT_RELEASEDIRECTORY -recurse -include "drop" | 
    ?{ $_.PSIsContainer } | 
    foreach { gci -Path $_.FullName -Recurse -include appsettings.json }
if($files)
{
    Write-Verbose -Verbose "Will apply $NewVersion to $($files.count) files."


    foreach ($file in $files) {
		$filecontent = Get-Content($file)
        attrib $file -r
        $filecontent -replace $VersionRegex, $NewVersion | Out-File $file
        Write-Verbose -Verbose "$file - release version applied"
    }
}
else
{
    Write-Warning "Found no files."
}
Write-Verbose -Verbose "Exiting script ApplyReleaseVersionToAssemblies.ps1"