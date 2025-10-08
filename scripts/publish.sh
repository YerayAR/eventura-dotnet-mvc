#!/usr/bin/env bash
set -euo pipefail
dotnet publish src/Eventura.Web/Eventura.Web.csproj -c Release -o out
