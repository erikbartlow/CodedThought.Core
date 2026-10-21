[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[a-z0-9][a-z0-9_-]*$')][string]$Username,
    [ValidatePattern('^[a-z0-9][a-z0-9._-]*$')][string]$Repository = 'google-calendar-pi',
    [ValidatePattern('^[A-Za-z0-9_][A-Za-z0-9_.-]{0,127}$')][string]$Tag = 'latest',
    [ValidateSet('linux/amd64','linux/arm64','linux/amd64,linux/arm64')][string]$Platform = 'linux/amd64',
    [switch]$Push
)
$ErrorActionPreference = 'Stop'
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Install Docker with Buildx first.' }
if (-not $Push -and $Platform.Contains(',')) { throw 'Multi-platform builds require -Push. Choose one platform for a local build.' }
$image = "docker.io/${Username}/${Repository}:${Tag}"
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$dockerArgs = @('buildx','build','--platform',$Platform,'--tag',$image,'--file',(Join-Path $root 'Dockerfile'))
if ($Push) { $dockerArgs += '--push' } else { $dockerArgs += '--load' }
$dockerArgs += $root
& docker @dockerArgs
if ($LASTEXITCODE -ne 0) { throw "Docker build failed with exit code $LASTEXITCODE." }
Write-Host "$(if ($Push) { 'Published' } else { 'Built' }) $image"
