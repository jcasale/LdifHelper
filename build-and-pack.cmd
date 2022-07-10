git clean -xfd -e .vs

mkdir artifacts

dotnet restore

dotnet build .\src\LdifHelper\LdifHelper.csproj -c Release
dotnet pack .\src\LdifHelper\LdifHelper.csproj -c Release --include-symbols

for /R %%x in (LdifHelper*.*nupkg) do copy "%%x" "artifacts/" /Y