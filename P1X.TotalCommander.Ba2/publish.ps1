cd $PSScriptRoot
dotnet publish -r win-x64 -p:NativeLib=Shared
copy $PSScriptRoot\bin\Release\net8.0\win-x64\publish\P1X.TotalCommander.Ba2.dll $PSScriptRoot\bin\Release\net8.0\win-x64\publish\P1X.TotalCommander.Ba2.wcx64
echo done!