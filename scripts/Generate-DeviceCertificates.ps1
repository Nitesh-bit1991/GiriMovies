# GiriMovies Certificate Authority Setup
# This script creates a local CA and generates client certificates for devices

param(
    [Parameter(Mandatory=$false)]
    [string]$DeviceName,
    
    [Parameter(Mandatory=$false)]
    [string]$DeviceType,
    
    [switch]$SetupCA,
    
    [string]$ComputerName = $env:COMPUTERNAME,
    [string]$OutputPath = ".\certificates"
)

# Resolve absolute path for output directory
$OutputPath = Resolve-Path $OutputPath -ErrorAction SilentlyContinue
if (-not $OutputPath) {
    $OutputPath = Join-Path (Get-Location) "certificates"
}

# Create output directory
if (!(Test-Path $OutputPath)) {
    New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null
    Write-Host "Created certificate directory: $OutputPath" -ForegroundColor Green
}

# Function to create CA certificate
function New-CACertificate {
    param([string]$OutputPath)
    
    $caName = "GiriMovies Local CA"
    $caSubject = "CN=$caName, O=GiriMovies, C=US"
    
    Write-Host "Creating Certificate Authority: $caName" -ForegroundColor Yellow
    
    # Create CA private key
    $caKey = [System.Security.Cryptography.RSA]::Create(4096)
    
    # Create CA certificate request
    $caReq = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
        $caSubject,
        $caKey,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256,
        [System.Security.Cryptography.RSASignaturePadding]::Pkcs1
    )
    
    # Add CA extensions
    $caReq.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new($true, $false, 0, $true))
    $caReq.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
        [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyCertSign -bor 
        [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::CrlSign, 
        $true
    ))
    
    # Create self-signed CA certificate
    $caCert = $caReq.CreateSelfSigned([System.DateTimeOffset]::UtcNow.AddDays(-1), [System.DateTimeOffset]::UtcNow.AddYears(5))
    
    # Save CA certificate and private key
    $caPath = Join-Path $OutputPath "GiriMovies-ca.pfx"
    $caPassword = "GiriMoviesCA2025!"
    
    $pfxBytes = $caCert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $caPassword)
    [System.IO.File]::WriteAllBytes($caPath, $pfxBytes)
    
    # Save CA public certificate for distribution
    $caCertPath = Join-Path $OutputPath "GiriMovies-ca.crt"
    $certBytes = $caCert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    $certPem = @"
-----BEGIN CERTIFICATE-----
$([Convert]::ToBase64String($certBytes, [Base64FormattingOptions]::InsertLineBreaks))
-----END CERTIFICATE-----
"@
    $certPem | Out-File -FilePath $caCertPath -Encoding UTF8
    
    Write-Host "CA Certificate created: $caCertPath" -ForegroundColor Green
    Write-Host "CA Private Key (PFX): $caPath (Password: $caPassword)" -ForegroundColor Green
    
    return $caCert
}

# Function to create client certificate
function New-ClientCertificate {
    param(
        [string]$DeviceName,
        [string]$DeviceType,
        [string]$ComputerName,
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$CACert,
        [string]$OutputPath
    )
    
    $clientSubject = "CN=$DeviceName, OU=$DeviceType, O=GiriMovies, C=US"
    
    Write-Host "Creating client certificate for: $DeviceName ($DeviceType)" -ForegroundColor Yellow
    
    # Create client private key
    $clientKey = [System.Security.Cryptography.RSA]::Create(2048)
    
    # Create client certificate request
    $clientReq = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
        $clientSubject,
        $clientKey,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256,
        [System.Security.Cryptography.RSASignaturePadding]::Pkcs1
    )
    
    # Add Subject Alternative Name
    $sanBuilder = [System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
    $sanBuilder.AddDnsName($ComputerName)
    $sanBuilder.AddDnsName("localhost")
    
    # Add local IP if available
    try {
        $localIP = (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias "Ethernet*" | Where-Object { $_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*" -or $_.IPAddress -like "172.*" } | Select-Object -First 1).IPAddress
        if ($localIP) {
            $sanBuilder.AddIpAddress([System.Net.IPAddress]::Parse($localIP))
        }
    } catch {
        Write-Warning "Could not determine local IP address"
    }
    
    $clientReq.CertificateExtensions.Add($sanBuilder.Build())
    
    # Add client authentication extension
    $clientReq.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
        [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor
        [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment,
        $true
    ))
    
    $clientAuthOid = [System.Security.Cryptography.Oid]::new("1.3.6.1.5.5.7.3.2") # Client Authentication
    $oidCollection = [System.Security.Cryptography.OidCollection]::new()
    $oidCollection.Add($clientAuthOid)
    $clientReq.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new(
        $oidCollection,
        $true
    ))
    
    # Generate serial number
    $serialBytes = New-Object byte[] 16
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($serialBytes)
    $rng.Dispose()
    
    # Create client certificate signed by CA
    $clientCert = $clientReq.Create(
        $CACert,
        [System.DateTimeOffset]::UtcNow.AddDays(-1),
        [System.DateTimeOffset]::UtcNow.AddYears(1),
        $serialBytes
    )
    
    # Combine with private key
    $clientCertWithKey = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::CopyWithPrivateKey($clientCert, $clientKey)
    
    # Save client certificate and private key
    $deviceFileName = $DeviceName -replace '[^a-zA-Z0-9]', '-'
    $clientPath = Join-Path $OutputPath "$deviceFileName.pfx"
    $clientPassword = "Device2025!"
    
    $clientPfxBytes = $clientCertWithKey.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $clientPassword)
    [System.IO.File]::WriteAllBytes($clientPath, $clientPfxBytes)
    
    # Save client certificate for distribution
    $clientCertPath = Join-Path $OutputPath "$deviceFileName.crt"
    $clientCertBytes = $clientCertWithKey.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    $clientCertPem = @"
-----BEGIN CERTIFICATE-----
$([Convert]::ToBase64String($clientCertBytes, [Base64FormattingOptions]::InsertLineBreaks))
-----END CERTIFICATE-----
"@
    $clientCertPem | Out-File -FilePath $clientCertPath -Encoding UTF8
    
    Write-Host "Client Certificate created: $clientCertPath" -ForegroundColor Green
    Write-Host "Client Private Key (PFX): $clientPath (Password: $clientPassword)" -ForegroundColor Green
    Write-Host "Certificate Thumbprint: $($clientCertWithKey.Thumbprint)" -ForegroundColor Cyan
    
    return @{
        Certificate = $clientCertWithKey
        PfxPath = $clientPath
        CertPath = $clientCertPath
        Password = $clientPassword
    }
}

# Main execution
try {
    Write-Host "GiriMovies Certificate Generator" -ForegroundColor Magenta
    Write-Host "===================================" -ForegroundColor Magenta
    
    # If SetupCA is specified, only create the CA and exit
    if ($SetupCA) {
        Write-Host "Setting up Certificate Authority only..." -ForegroundColor Yellow
        $caCert = New-CACertificate -OutputPath $OutputPath
        Write-Host "`nCA setup completed successfully!" -ForegroundColor Green
        Write-Host "CA certificate created: $(Join-Path $OutputPath 'GiriMovies-ca.crt')" -ForegroundColor Cyan
        Write-Host "CA private key (PFX): $(Join-Path $OutputPath 'GiriMovies-ca.pfx')" -ForegroundColor Cyan
        Write-Host "`nYou can now distribute the CA certificate (GiriMovies-ca.crt) to all client computers." -ForegroundColor Yellow
        exit 0
    }
    
    # Validate required parameters for client certificate generation
    if (-not $DeviceName) {
        Write-Error "DeviceName parameter is required for client certificate generation"
        exit 1
    }
    if (-not $DeviceType) {
        Write-Error "DeviceType parameter is required for client certificate generation"
        exit 1
    }
    
    # Check if CA certificate exists
    $caPath = Join-Path $OutputPath "GiriMovies-ca.pfx"
    $caPassword = "GiriMoviesCA2025!"
    
    if (Test-Path $caPath) {
        Write-Host "Loading existing CA certificate..." -ForegroundColor Yellow
        $caCert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($caPath, $caPassword, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
    } else {
        $caCert = New-CACertificate -OutputPath $OutputPath
    }
    
    # Create client certificate
    $clientResult = New-ClientCertificate -DeviceName $DeviceName -DeviceType $DeviceType -ComputerName $ComputerName -CACert $caCert -OutputPath $OutputPath
    
    # Create installation instructions
    $instructionsPath = Join-Path $OutputPath "INSTALL-$($DeviceName -replace '[^a-zA-Z0-9]', '-').md"
    $instructions = @"
# Certificate Installation Instructions for $DeviceName

## Device Information
- **Device Name**: $DeviceName
- **Device Type**: $DeviceType  
- **Computer Name**: $ComputerName
- **Certificate Thumbprint**: $($clientResult.Certificate.Thumbprint)

## Installation Steps

### 1. Install CA Certificate (One-time setup)
````powershell
# Import the CA certificate to Trusted Root Certification Authorities
Import-Certificate -FilePath "GiriMovies-ca.crt" -CertStoreLocation "Cert:\LocalMachine\Root"
````

### 2. Install Client Certificate
````powershell
# Import the client certificate to Personal store
Import-PfxCertificate -FilePath "$($clientResult.PfxPath | Split-Path -Leaf)" -CertStoreLocation "Cert:\CurrentUser\My" -Password (ConvertTo-SecureString "$($clientResult.Password)" -AsPlainText -Force)
````

### 3. Configure Browser/Application
- **Certificate File**: $($clientResult.CertPath | Split-Path -Leaf)
- **Private Key File**: $($clientResult.PfxPath | Split-Path -Leaf) 
- **Password**: $($clientResult.Password)

### 4. Test Certificate
Navigate to: https://your-GiriMovies-server:5001/api/certificates/authenticate

## Files Created
- **CA Certificate**: GiriMovies-ca.crt
- **Client Certificate**: $($clientResult.CertPath | Split-Path -Leaf)
- **Client PFX**: $($clientResult.PfxPath | Split-Path -Leaf)

## Security Notes
- Keep the PFX file and password secure
- Do not share the private key
- Certificate expires: $($clientResult.Certificate.NotAfter.ToString("yyyy-MM-dd"))

"@
    
    $instructions | Out-File -FilePath $instructionsPath -Encoding UTF8
    
    Write-Host "`nInstallation instructions created: $instructionsPath" -ForegroundColor Green
    Write-Host "`nCertificate generation completed successfully!" -ForegroundColor Magenta
    
} catch {
    Write-Error "Error generating certificates: $($_.Exception.Message)"
    exit 1
}