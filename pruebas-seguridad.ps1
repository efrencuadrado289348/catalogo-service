# Pruebas de seguridad - Segunda entrega (Identity + Catalog + Pedidos)
#
# Requisitos: los 6 contenedores arriba (docker compose up -d --build).
# Ejecutar desde PowerShell:
#   powershell -ExecutionPolicy Bypass -File .\pruebas-seguridad.ps1
#
# Token expirado (opcional): en docker-compose.yml poner Jwt__ExpirationMinutes: "1" en identity-api,
# reconstruir identity-api y ejecutar:  .\pruebas-seguridad.ps1 -ProbarExpirado

param([switch]$ProbarExpirado)

$identity = "http://localhost:5081"
$catalog  = "http://localhost:5080"
$orders   = "http://localhost:5082"

function Get-Token($email, $pass) {
    $body = @{ email = $email; password = $pass } | ConvertTo-Json
    (Invoke-RestMethod -Method Post -Uri "$identity/api/auth/login" -ContentType "application/json" -Body $body).accessToken
}

function Probar($n, $descripcion, $metodo, $url, $token, $body, $esperado) {
    $headers = @{}
    if ($token) { $headers.Authorization = "Bearer $token" }
    try {
        $r = Invoke-WebRequest -Method $metodo -Uri $url -Headers $headers -Body $body -ContentType "application/json" -UseBasicParsing
        $codigo = [int]$r.StatusCode
    } catch {
        $codigo = [int]$_.Exception.Response.StatusCode
    }
    $color = if ($codigo -eq $esperado) { "Green" } else { "Red" }
    Write-Host ("{0,2}. {1,-48} esperado {2} -> obtuvo {3}" -f $n, $descripcion, $esperado, $codigo) -ForegroundColor $color
}

Write-Host "`n== Login en Identity ==" -ForegroundColor Cyan
$cliente  = Get-Token "cliente@ecommerce.local"  "Cliente123!"
$vendedor = Get-Token "vendedor@ecommerce.local" "Vendedor123!"
$admin    = Get-Token "admin@ecommerce.local"    "Admin123!"
Write-Host "Tokens obtenidos para cliente, vendedor y admin."
Write-Host ("Token cliente: {0}..." -f $cliente.Substring(0, 40))

$alterado = $cliente.Substring(0, $cliente.Length - 2) + "xx"
$sufijo   = Get-Date -Format "HHmmss"

Write-Host "`n== Catalog ==" -ForegroundColor Cyan
Probar 1 "GET products sin token"              GET  "$catalog/api/products" $null     $null 401
Probar 2 "GET products con token alterado"     GET  "$catalog/api/products" $alterado $null 401
Probar 3 "GET products como Cliente"           GET  "$catalog/api/products" $cliente  $null 200

$cat = @{ name = "Categoria $sufijo"; description = "Prueba" } | ConvertTo-Json
Probar 4 "POST categories como Cliente"        POST "$catalog/api/categories" $cliente  $cat 403
Probar 5 "POST categories como Vendedor"       POST "$catalog/api/categories" $vendedor $cat 403
Probar 6 "POST categories como Admin"          POST "$catalog/api/categories" $admin    $cat 201

# Datos para las pruebas siguientes
$catId = (Invoke-RestMethod -Method Post -Uri "$catalog/api/categories" -Headers @{ Authorization = "Bearer $admin" } -ContentType "application/json" `
    -Body (@{ name = "Cat pedidos $sufijo" } | ConvertTo-Json)).id
$prod = @{ name = "Portatil $sufijo"; description = "Prueba"; price = 2500000; currency = "COP"; stock = 10; categoryId = $catId } | ConvertTo-Json

Probar 7 "POST products como Cliente"          POST "$catalog/api/products" $cliente  $prod 403
Probar 8 "POST products como Vendedor"         POST "$catalog/api/products" $vendedor $prod 201

$prodId = (Invoke-RestMethod -Method Post -Uri "$catalog/api/products" -Headers @{ Authorization = "Bearer $vendedor" } -ContentType "application/json" `
    -Body (@{ name = "Portatil B $sufijo"; description = "Prueba"; price = 2500000; currency = "COP"; stock = 10; categoryId = $catId } | ConvertTo-Json)).id

Write-Host "`n== Pedidos (consulta Catalog reenviando el token) ==" -ForegroundColor Cyan
$pedido = @{ items = @(@{ productId = $prodId; quantity = 2 }) } | ConvertTo-Json -Depth 3
Probar 9  "POST orders sin token"              POST "$orders/api/orders" $null     $pedido 401
Probar 10 "POST orders como Vendedor"          POST "$orders/api/orders" $vendedor $pedido 403
Probar 11 "POST orders como Cliente"           POST "$orders/api/orders" $cliente  $pedido 201

if ($ProbarExpirado) {
    Write-Host "`n== Token expirado (esperando 70 s; Catalog tiene ClockSkew 0) ==" -ForegroundColor Cyan
    $temporal = Get-Token "cliente@ecommerce.local" "Cliente123!"
    Start-Sleep -Seconds 70
    Probar 12 "GET products con token expirado"  GET "$catalog/api/products" $temporal $null 401
}

Write-Host ""
