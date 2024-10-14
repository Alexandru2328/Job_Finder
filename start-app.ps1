cd "C:\Users\alexa\source\repos\pastebin\Job_Finder"

$visualStudioProcess = Start-Process "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" -ArgumentList "Job_Finder.sln" -PassThru

Start-Sleep -Seconds 15

Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.SendKeys]::SendWait("{F5}")

Start-Sleep -Seconds 800

Stop-Process -Id $visualStudioProcess.Id
