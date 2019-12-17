param($nssmDir, $processName, $filePath, $environment, $userName, $password)

$nssmExe = "$nssmDir\nssm.exe"

Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "install $processName $filePath $environment --user $userName --password $password"
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "start $processName"