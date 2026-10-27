#!/usr/bin/env bash
set -euo pipefail
repository_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
cd -- "$repository_directory"
dotnet run --project tools/Codestyle/Checks/MadSlime.CodeStyle.Checks.csproj --configuration Release -p:NuGetAudit=false
cp tools/Codestyle/Analyzer/bin/Release/netstandard2.0/MadSlime.CodeStyle.dll \
    Assets/MadSlime/Scripts/Assemblies/MadSlime.Core/Analyzers/MadSlime.CodeStyle.dll
dotnet run --project tools/Codestyle/Checks/MadSlime.CodeStyle.Checks.csproj --configuration Release --no-build -- --installed
