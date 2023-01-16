[CmdletBinding()]
param
(
    [Parameter(Position=0)]
    [string]
    $Path = (Join-Path $PSScriptRoot 'version.props')
)

Set-StrictMode -Version Latest

if (Test-Path $Path)
{
    Remove-Item $Path -Force
}

try
{
    $result = & git.exe describe HEAD --tags --long
}
catch
{
    throw
}

if ($LASTEXITCODE -ne 0)
{
    throw 'Git failed with exit code: "{0}".' -f $LASTEXITCODE
}

$informationalVersion = $result.TrimStart('v')
$major, $minor, $build, $revision = $informationalVersion.Split('-')[0].Split('.')

Write-Verbose ('Using git version {0}.' -f $informationalVersion)

$content = @'
<Project>
  <PropertyGroup>
    <AssemblyVersion>{0}.{1}.{2}.{3}</AssemblyVersion>
    <FileVersion>{0}.{1}.{2}.{3}</FileVersion>
    <Version>{0}.{1}.{2}</Version>
    <InformationalVersion>{4}</InformationalVersion>
  </PropertyGroup>
</Project>
'@ -f $major, $minor, $build, $revision, $informationalVersion

Set-Content -Value $content -Path $Path