# generate-migration-script.ps1
# Gera um script SQL idempotente com todas as migrations do projeto.
# O arquivo gerado pode ser aplicado diretamente no banco de produção via psql ou
# qualquer cliente SQL, e é seguro para re-execução (cada migration é verificada
# em __EFMigrationsHistory antes de ser aplicada).
#
# Uso:
#   .\scripts\generate-migration-script.ps1
#   .\scripts\generate-migration-script.ps1 -Output "deploy\migrations.sql"

param(
    [string]$Output = "migrations.sql"
)

$ErrorActionPreference = "Stop"

$repoRoot      = Split-Path -Parent $PSScriptRoot
$infraProject  = Join-Path $repoRoot "src\Voltiq.Infrastructure"
$startupProject = Join-Path $repoRoot "src\Voltiq.API"
$outputPath    = Join-Path $repoRoot $Output

Write-Host "▶ Gerando script SQL idempotente..." -ForegroundColor Cyan

dotnet ef migrations script `
    --idempotent `
    --project $infraProject `
    --startup-project $startupProject `
    --output $outputPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Falha ao gerar o script SQL."
    exit 1
}

Write-Host "✅ Script gerado em: $outputPath" -ForegroundColor Green
