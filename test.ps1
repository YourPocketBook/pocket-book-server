& .\nuget install opencover -outputdirectory ./tools -excludeversion
& .\nuget install reportgenerator -outputdirectory ./tools -excludeversion

& .\tools\OpenCover\tools\opencover.console.exe -register:user -filter:"+[PocketBookServer]* -[PocketBookServer]PocketBookServer.Migrations.* -[PocketBookServer]PocketBookServer.Startup* -[PocketBookServer]PocketBookServer.Program -[PocketBookServer]PocketBookServer.Data.*" -target:"c:\program files\dotnet\dotnet.exe" -targetargs:"test" -output:coverage.xml -oldStyle -returntargetcode
& .\tools\reportgenerator\tools\net47\reportgenerator.exe -reports:coverage.xml -targetdir:.\coverage -historydir:.\coverage-history
