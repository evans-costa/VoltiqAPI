#!/usr/bin/env bash
# generate-migration-script.sh
# Gera um script SQL idempotente com todas as migrations do projeto.
# Seguro para re-execucao — cada migration e verificada em __EFMigrationsHistory
# antes de ser aplicada.
#
# Uso:
#   ./scripts/generate-migration-script.sh
#   ./scripts/generate-migration-script.sh deploy/migrations.sql

set -euo pipefail

OUTPUT="${1:-migrations.sql}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
INFRA_PROJECT="$REPO_ROOT/src/Voltiq.Infrastructure"
STARTUP_PROJECT="$REPO_ROOT/src/Voltiq.API"
OUTPUT_PATH="$REPO_ROOT/$OUTPUT"

if ! dotnet ef --version &>/dev/null; then
  echo "dotnet-ef nao encontrado. Instalando..." >&2
  dotnet tool install --global dotnet-ef
fi

echo "Gerando script SQL idempotente..."

dotnet ef migrations script \
  --idempotent \
  --project "$INFRA_PROJECT" \
  --startup-project "$STARTUP_PROJECT" \
  --output "$OUTPUT_PATH"

echo "Script gerado em: $OUTPUT_PATH"
