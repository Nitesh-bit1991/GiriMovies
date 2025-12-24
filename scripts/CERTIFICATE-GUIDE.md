# GiriMovies Certificate Generation Guide

## 🎯 **Purpose**
Generate client certificates for device authentication in the GiriMovies offline multi-device system.

## 📋 **Prerequisites**
- **Windows PowerShell 5.1+** or **PowerShell 7+**
- **Administrative privileges** (for certificate installation)
- **.NET Framework 4.7.2+** or **.NET Core 3.1+**

## 🚀 **Quick Start**

### **Method 1: Using Batch File (Recommended)**
1. **Open Command Prompt** as Administrator
2. **Navigate to scripts directory**:
   ```cmd
   cd "D:\Dev\GiriMovies\scripts"
   ```
3. **Run certificate generator**:
   ```cmd
   generate-certificates.bat
   ```
4. **Follow the interactive prompts**:
   - Enter device name (e.g., "John-Laptop")
   - Select device type (Computer, Laptop, Tablet, Mobile, TV)
   - Confirm generation

### **Method 2: Direct PowerShell**
1. **Open PowerShell** as Administrator
2. **Navigate to scripts directory**:
   ```powershell
   cd "D:\Dev\GiriMovies\scripts"
   ```
3. **Run with parameters**:
   ```powershell
   .\Generate-DeviceCertificates.ps1 -DeviceName "John-Laptop" -DeviceType "Laptop"
   ```

## 📁 **Generated Files**

After successful execution, you'll find these files in the `certificates` folder:

### **Certificate Authority Files**
- `GiriMovies-ca.crt` - Public CA certificate (distribute to all devices)
- `GiriMovies-ca.pfx` - CA private key (keep secure)

### **Device-Specific Files**  
- `{DeviceName}.crt` - Device public certificate
- `{DeviceName}.pfx` - Device certificate with private key
- `{DeviceName}.key` - Device private key

## 🔧 **Installation Steps**

### **1. Install CA Certificate (One-time setup per device)**
```powershell
# Import the CA certificate to Trusted Root Certification Authorities
Import-Certificate -FilePath "GiriMovies-ca.crt" -CertStoreLocation "Cert:\LocalMachine\Root"
```

### **2. Install Device Certificate**
```powershell
# Import the device certificate to Personal store
Import-PfxCertificate -FilePath "John-Laptop.pfx" -CertStoreLocation "Cert:\CurrentUser\My" -Password (ConvertTo-SecureString "DevicePassword123!" -AsPlainText -Force)
```

### **3. Configure GiriMovies Client**
1. **Copy certificate files** to client device
2. **Update client configuration** with certificate paths
3. **Configure application** to use client certificates

## 🐛 **Troubleshooting**

### **Error: "The argument Generate-DeviceCertificates.ps1 for the -File parameter does not exist"**
**Solution**: Make sure you're running the batch file from the correct directory:
```cmd
cd "D:\Dev\GiriMovies\scripts"
generate-certificates.bat
```

### **Error: "Execution Policy Restricted"**
**Solution**: Run PowerShell as Administrator and set execution policy:
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
```

### **Error: "Access Denied"**  
**Solution**: Run Command Prompt or PowerShell as Administrator.

### **Error: "Certificate installation failed"**
**Solution**: Verify you have administrative privileges and try manual installation:
```powershell
# Check current certificates
Get-ChildItem Cert:\LocalMachine\Root | Where-Object {$_.Subject -like "*GiriMovies*"}

# Manual import
$cert = Import-Certificate -FilePath "GiriMovies-ca.crt" -CertStoreLocation "Cert:\LocalMachine\Root"
Write-Host "Imported: $($cert.Subject)"
```

## 📊 **Device Types Supported**

| Type | Description | Use Case |
|------|-------------|----------|
| Computer | Desktop PC | Main viewing station |
| Laptop | Portable computer | Mobile viewing |
| Tablet | Touch device | Casual viewing |
| Mobile | Smartphone | Quick access |
| TV | Smart TV / Set-top box | Living room viewing |

## 🔐 **Security Best Practices**

### **Certificate Management**
- ✅ **Keep PFX files secure** - Never share device private keys
- ✅ **Use strong passwords** - Generated passwords are complex
- ✅ **Regular rotation** - Replace certificates annually
- ✅ **Revocation capability** - Remove compromised devices

### **CA Certificate Protection**
- 🔒 **Backup CA files** securely
- 🔒 **Limit CA access** to administrators only
- 🔒 **Monitor certificate usage**
- 🔒 **Log certificate operations**

### **Default Passwords**
- **CA Password**: `GiriMoviesCA2025!`
- **Device Password**: `DevicePassword123!`
- **Production**: Change these in production environments

## 🌐 **Multi-Device Setup Example**

### **Scenario: Family with 3 devices**
1. **Living Room PC** (Computer):
   ```cmd
   generate-certificates.bat
   # Device: "LivingRoom-PC", Type: "Computer"
   ```

2. **Mom's Laptop** (Laptop):
   ```cmd
   generate-certificates.bat  
   # Device: "Mom-Laptop", Type: "Laptop"
   ```

3. **Kids Tablet** (Tablet):
   ```cmd
   generate-certificates.bat
   # Device: "Kids-Tablet", Type: "Tablet"
   ```

### **Result**: 
- All devices can access GiriMovies
- Watch progress syncs between devices
- Each device has unique authentication
- Admin can manage device access centrally

## 📝 **Next Steps After Certificate Generation**

1. **Install certificates** on each device
2. **Configure GiriMovies client** with certificate settings
3. **Test authentication** by accessing the application
4. **Verify device tracking** in the admin panel
5. **Test cross-device synchronization**

## 🎬 **Integration with GiriMovies**

The generated certificates integrate with:
- **Authentication Controller** (`/api/certificates/authenticate`)
- **Device Tracking** (UserSessions table)
- **Certificate Management** (CertificatesController)
- **Multi-Device Sync** (cross-device progress tracking)

---

**Certificate Generation Status**: ✅ Ready for production use

For additional support, check the main GiriMovies documentation or the debug endpoints in the API.