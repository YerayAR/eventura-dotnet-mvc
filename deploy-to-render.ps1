# Script de Despliegue Automático de Eventura a Render
# Ejecutar: .\deploy-to-render.ps1

Write-Host "🚀 Iniciando despliegue de Eventura en Render..." -ForegroundColor Cyan

# Verificar que Render CLI esté instalado
if (-not (Get-Command "render" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Render CLI no encontrado. Instalando..." -ForegroundColor Yellow
    Write-Host "Visita: https://render.com/docs/cli" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Instalar con:" -ForegroundColor Green
    Write-Host "npm install -g @render/cli" -ForegroundColor White
    exit 1
}

# Login a Render
Write-Host "`n🔐 Iniciando sesión en Render..." -ForegroundColor Cyan
render login

# Verificar render.yaml
if (-not (Test-Path "render.yaml")) {
    Write-Host "❌ No se encontró render.yaml" -ForegroundColor Red
    exit 1
}

# Desplegar
Write-Host "`n🎯 Desplegando Eventura..." -ForegroundColor Cyan
render blueprint launch

Write-Host "`n✅ Despliegue iniciado!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URL de acceso (cuando esté listo):" -ForegroundColor Cyan
Write-Host "https://eventura-app.onrender.com" -ForegroundColor White
Write-Host ""
Write-Host "👤 Credenciales por defecto:" -ForegroundColor Yellow
Write-Host "Usuario: admin" -ForegroundColor White
Write-Host "Password: AdminPass123!" -ForegroundColor White
Write-Host ""
Write-Host "⏱️ Tiempo estimado: 3-5 minutos" -ForegroundColor Yellow
