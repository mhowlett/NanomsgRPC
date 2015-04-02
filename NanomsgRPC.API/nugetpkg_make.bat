md nugetpkg
md nugetpkg\lib
md nugetpkg\lib\net40
md nugetpkg\tools
md nugetpkg\tools\net40
copy NanomsgRPC.API.nuspec nugetpkg
copy bin\Release\NanomsgRPC.API.dll nugetpkg\lib\net40
copy install.ps1 nugetpkg\tools\net40
