param($nssmDir, $processName, $filePath)

$nssmExe = "$nssmDir\nssm.exe"

Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "install $processName dotnet $filePath"
Start-Process -FilePath $nssmExe -Wait -NoNewWindow -ArgumentList "start $processName"