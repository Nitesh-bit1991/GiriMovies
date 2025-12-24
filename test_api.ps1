# Test script to debug watch progress issues
$baseUrl = "https://localhost:55428"

# Disable SSL verification for testing
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

Write-Host "=== GiriMovies API Test ===" -ForegroundColor Cyan

# Step 1: Register user
Write-Host "`n1. Registering user 'nitesh@test.com'..." -ForegroundColor Yellow
$registerData = @{
    email = "nitesh@test.com"
    password = "Test123!"
    name = "Nitesh"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json" -ErrorAction Stop
    Write-Host "✅ User registered successfully!" -ForegroundColor Green
    Write-Host "Token: $($registerResponse.token.Substring(0,20))..." -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "ℹ️ User already exists, trying to login..." -ForegroundColor Yellow
    } else {
        Write-Host "❌ Registration failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Step 2: Login user
Write-Host "`n2. Logging in user..." -ForegroundColor Yellow
$deviceInfo = @{
    macAddress = "AA:BB:CC:DD:EE:FF"
    processorId = "CPU-TEST123"
    machineGuid = "12345678-1234-1234-1234-123456789ABC"
    computerName = "TEST-COMPUTER"
    userName = "TestUser"
    osVersion = "Windows 10"
    localIP = "192.168.1.100"
    timeZone = "Central European Standard Time"
    userAgent = "PowerShell-Test"
    screenResolution = "1920x1080"
    deviceName = "Test Computer"
}

$loginData = @{
    email = "nitesh@test.com"
    password = "Test123!"
    deviceType = "Computer"
    deviceInfo = $deviceInfo
} | ConvertTo-Json -Depth 3

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "Device ID: $($loginResponse.deviceId)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Get movies
Write-Host "`n3. Fetching movies..." -ForegroundColor Yellow
$headers = @{ "Authorization" = "Bearer $token" }

try {
    $movies = Invoke-RestMethod -Uri "$baseUrl/api/movies" -Method GET -Headers $headers
    Write-Host "✅ Found $($movies.Count) movies" -ForegroundColor Green
    
    if ($movies.Count -gt 0) {
        $movie = $movies[0]
        Write-Host "First movie: '$($movie.title)' (ID: $($movie.id))" -ForegroundColor Gray
        Write-Host "Current position: $($movie.currentPositionInSeconds) seconds" -ForegroundColor Gray
        Write-Host "Progress: $($movie.progressPercentage)%" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Failed to fetch movies: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 4: Update watch progress if we have movies
if ($movies.Count -gt 0) {
    Write-Host "`n4. Updating watch progress..." -ForegroundColor Yellow
    $movieId = $movies[0].id
    
    $progressData = @{
        movieId = $movieId
        currentPositionInSeconds = 53
        deviceType = "Computer"
    } | ConvertTo-Json
    
    try {
        $progressResponse = Invoke-RestMethod -Uri "$baseUrl/api/watchprogress" -Method POST -Body $progressData -ContentType "application/json" -Headers $headers
        Write-Host "✅ Progress updated successfully!" -ForegroundColor Green
        Write-Host "Saved position: $($progressResponse.currentPositionInSeconds) seconds" -ForegroundColor Gray
        Write-Host "Device: $($progressResponse.lastWatchedDevice)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ Failed to update progress: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Response: $($_.Exception.Response | ConvertTo-Json)" -ForegroundColor Red
    }
    
    # Step 5: Get watch progress
    Write-Host "`n5. Retrieving watch progress..." -ForegroundColor Yellow
    try {
        $savedProgress = Invoke-RestMethod -Uri "$baseUrl/api/watchprogress/movie/$movieId" -Method GET -Headers $headers
        Write-Host "✅ Retrieved progress!" -ForegroundColor Green
        Write-Host "Position: $($savedProgress.currentPositionInSeconds) seconds" -ForegroundColor Gray
        Write-Host "Last watched: $($savedProgress.lastWatchedAt)" -ForegroundColor Gray
        Write-Host "Device: $($savedProgress.lastWatchedDevice)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ Failed to retrieve progress: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Step 6: Get movie with progress
    Write-Host "`n6. Getting movie with embedded progress..." -ForegroundColor Yellow
    try {
        $movieWithProgress = Invoke-RestMethod -Uri "$baseUrl/api/movies/$movieId" -Method GET -Headers $headers
        Write-Host "✅ Movie retrieved!" -ForegroundColor Green
        Write-Host "Title: $($movieWithProgress.title)" -ForegroundColor Gray
        Write-Host "Current position: $($movieWithProgress.currentPositionInSeconds) seconds" -ForegroundColor Gray
        Write-Host "Progress: $($movieWithProgress.progressPercentage)%" -ForegroundColor Gray
        Write-Host "Last watched device: $($movieWithProgress.lastWatchedDevice)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ Failed to retrieve movie: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan