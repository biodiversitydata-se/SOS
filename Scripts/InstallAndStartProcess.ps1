param($nssmDir, $processName, $filePath, $environment)

$nssmExe = "$nssmDir\nssm.exe"

Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "install $processName $filePath $environment"
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "start $processName"