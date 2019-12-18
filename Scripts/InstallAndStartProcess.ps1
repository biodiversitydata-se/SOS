param($nssmDir, $processName, $filePath, $environment, $userName, $password)

$nssmExe = "$nssmDir\nssm.exe"

Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "install $processName $filePath $environment"
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "set $processName ObjectName $userName $password" 
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "start $processName" 