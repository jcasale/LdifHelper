git clean -xfd

mkdir artifacts

dotnet restore

dotnet build .\src\LdifHelper\LdifHelper.csproj -c Release
dotnet pack .\src\LdifHelper\LdifHelper.csproj -c Release

for /R %%x in (LdifHelper*.nupkg) do copy "%%x" "artifacts/" /Y
