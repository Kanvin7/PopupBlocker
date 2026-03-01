Get-ChildItem obj,bin -Recurse | Remove-Item -Recurse
dotnet restore .\PopupBlocker.sln
&"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\PopupBlocker.sln -t:Build -p:Configuration=Release -v:m
Remove-Item .\publish -Recurse
Copy-Item .\PopupBlocker\bin\Release\net8.0-windows .\publish\PopupBlocker -Recurse
New-Item .\publish\PopupBlocker\document -Type Directory
Copy-Item .\README.md .\publish\PopupBlocker\document
Copy-Item .\VersionLog.md .\publish\PopupBlocker\document
New-Item .\publish\PopupBlocker\license -Type Directory
Copy-Item .\LICENSE.txt .\publish\PopupBlocker\license
Copy-Item .\THIRD_PARTY_NOTICES.md .\publish\PopupBlocker\license
tar -czvf .\publish\PopupBlocker.tar.gz -C .\publish PopupBlocker