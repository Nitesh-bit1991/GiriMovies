# Certificate Installation Instructions for NGLaptop

## Device Information
- **Device Name**: NGLaptop
- **Device Type**: Computer  
- **Computer Name**: LONB1047638
- **Certificate Thumbprint**: F5F3999EDC3D6E514904908D1C135796FF395116

## Installation Steps

### 1. Install CA Certificate (One-time setup)
``powershell
# Import the CA certificate to Trusted Root Certification Authorities
Import-Certificate -FilePath "GiriMovies-ca.crt" -CertStoreLocation "Cert:\LocalMachine\Root"
``

### 2. Install Client Certificate
``powershell
# Import the client certificate to Personal store
Import-PfxCertificate -FilePath "NGLaptop.pfx" -CertStoreLocation "Cert:\CurrentUser\My" -Password (ConvertTo-SecureString "Device2025!" -AsPlainText -Force)
``

### 3. Configure Browser/Application
- **Certificate File**: NGLaptop.crt
- **Private Key File**: NGLaptop.pfx 
- **Password**: Device2025!

### 4. Test Certificate
Navigate to: https://your-GiriMovies-server:5001/api/certificates/authenticate

## Files Created
- **CA Certificate**: GiriMovies-ca.crt
- **Client Certificate**: NGLaptop.crt
- **Client PFX**: NGLaptop.pfx

## Security Notes
- Keep the PFX file and password secure
- Do not share the private key
- Certificate expires: 2026-11-18

