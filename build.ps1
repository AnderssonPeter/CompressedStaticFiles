choco install dotnetcore-sdk --no-progress --confirm --version 2.2.101
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release