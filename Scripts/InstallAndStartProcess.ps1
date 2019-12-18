param($nssmDir, $processName, $filePath, $environment, $userName, $password)

$nssmExe = "$nssmDir\nssm.exe"
$credentials = New-Object System.Management.Automation.PSCredential -ArgumentList @($userName, (ConvertTo-SecureString -String $password -AsPlainText -Force))

Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "install $processName $filePath $environment --user $userName --password $password"
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "start $processName" -Credential ($credentials)