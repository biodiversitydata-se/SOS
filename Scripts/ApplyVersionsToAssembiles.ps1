##-----------------------------------------------------------------------
## <copyright file="ApplyVersionToAssemblies.ps1">(c) Microsoft Corporation. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
##-----------------------------------------------------------------------
# Look for a Build_0_0_0 pattern in the build number. 
# If found use it to BuildVersion the appsettings.json.
#
# For example, if the 'Build number format' build process parameter 
# $(BuildDefinitionName)_$(Date:yyyyMMdd)_$(Rev:rr)_$(BuildID). Note that BuildDefinitionName must ends with "Build"
# then your build numbers come out like this:
# "<Name>Build_20130719_1_1222"
# This script would then apply version Build_20130719_1_1222 to your assemblies.
# This regexp pattern must differ from the pattern in ApplyVersionToAssemblies.ps1 if not incorrect "parameter" appsettings.json in will be set.
# TFS source directory must include folder "src", it's below that folder structure appsettings.json is to be found.

# Enable -Verbose option
[CmdletBinding()]

# Regular expression pattern to find the version in the build number 
# and then apply it to the assemblies  $(BuildDefinitionName)_$(Date:yyyyMMdd)_$(Rev:rr)_$(BuildID)
$VersionRegex = "[B|b][u][i][l][d].\d+.\d+.\d+"

Write-Verbose -Verbose "Entering script ApplyVersionToAssemblies.ps1"

# If this script is not running on a build server, remind user to 
# set environment variables so that this script can be debugged
if(-not ($Env:BUILD_SOURCESDIRECTORY -and $Env:BUILD_BUILDNUMBER))
{
    Write-Error "You must set the following environment variables"
    Write-Error "to test this script interactively."
    Write-Host '$Env:BUILD_SOURCESDIRECTORY - For example, enter something like:'
    Write-Host '$Env:BUILD_SOURCESDIRECTORY = "C:\dev\Asta\<Name>"'
    Write-Host '$Env:BUILD_BUILDNUMBER - For example, enter something like:'
    Write-Host '$Env:BUILD_BUILDNUMBER = "<Name>_000000_00_0000"'
    exit 1
}

# Make sure path to source code directory is available
if (-not $Env:BUILD_SOURCESDIRECTORY)
{
    Write-Error ("BUILD_SOURCESDIRECTORY environment variable is missing.")
    exit 1
}
elseif (-not (Test-Path $Env:BUILD_SOURCESDIRECTORY))
{
    Write-Error "BUILD_SOURCESDIRECTORY does not exist: $Env:BUILD_SOURCESDIRECTORY"
    exit 1
}
Write-Verbose -Verbose "BUILD_SOURCESDIRECTORY: $Env:BUILD_SOURCESDIRECTORY"

# Make sure there is a build number
if (-not $Env:BUILD_BUILDNUMBER)
{
    Write-Error ("BUILD_BUILDNUMBER environment variable is missing.")
    exit 1
}
Write-Verbose -Verbose "BUILD_BUILDNUMBER: $Env:BUILD_BUILDNUMBER"

# Get and validate the version data
$VersionData = [regex]::matches($Env:BUILD_BUILDNUMBER,$VersionRegex)
switch($VersionData.Count)
{
   0        
      { 
         Write-Error "Could not find version number data in BUILD_BUILDNUMBER."
         exit 1
      }
   1 {}
   default 
      { 
         Write-Warning "Found more than instance of version data in BUILD_BUILDNUMBER." 
         Write-Warning "Will assume first instance is version."
      }
}
$NewVersion = $VersionData[0]
Write-Verbose -Verbose "Build version: $NewVersion"

$BuildDate = ($NewVersion -split '_')[1]
Write-Verbose -Verbose "Build date: $BuildDate"

$Revision = ($NewVersion -split '_')[2]
Write-Verbose -Verbose "Build revision: $Revision"

$BuildId = ($NewVersion -split '_')[3]
Write-Verbose -Verbose "Build id: $BuildId"

# Apply the version to the assembly property files
$files = gci $Env:BUILD_SOURCESDIRECTORY -recurse -include "Applications" | 
    ?{ $_.PSIsContainer } | 
    foreach { gci -Path $_.FullName -Recurse -include appsettings.json }
if($files)
{
    Write-Verbose -Verbose "Will apply $NewVersion to $($files.count) files."


    foreach ($file in $files) {
		$filecontent = Get-Content($file)
        attrib $file -r
        $filecontent -replace $VersionRegex, $NewVersion | Out-File $file
        Write-Verbose -Verbose "$file - version applied"
    }
}
else
{
    Write-Warning "Found no files."
}
Write-Verbose -Verbose "Exiting script ApplyVersionToAssemblies.ps1"