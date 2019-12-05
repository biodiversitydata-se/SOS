param($nssmDir, $processName)

$nssmExe = "$nssmDir\nssm.exe"

Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "stop $processName"
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "remove $processName confirm" 
