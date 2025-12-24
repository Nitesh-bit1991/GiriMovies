# Watch Progress Debugging Guide

## 🐛 **Issue Identified**
User "Nitesh" watches "The Shawshank Redemption" for 5 minutes, but when returning later, the movie shows "start from beginning" instead of resuming from where left off.

## 🔍 **Root Cause Analysis**

### **1. Session Token Issue**
- **Problem**: Session tokens were only generated for new sessions, not updated for existing sessions
- **Impact**: Watch progress API couldn't find the correct device session
- **Fix**: Updated `AuthController.cs` to always generate session tokens

### **2. Device Session Lookup**
- **Problem**: Watch progress controller was looking for sessions with `SessionToken IS NULL`
- **Impact**: Device information wasn't being linked to watch progress
- **Fix**: Enhanced session token retrieval from JWT claims

## 🧪 **Testing Steps**

### **Step 1: Verify Session Creation**
1. **Login as Nitesh** with email and password
2. **Check Debug Endpoint**: `GET /api/watchprogress/debug/session`
3. **Verify**: Session token is not null and device info is captured

### **Step 2: Test Watch Progress Saving**
1. **Start watching movie**: Send POST request to `/api/watchprogress`
   ```json
   {
     "movieId": 1,
     "currentPositionInSeconds": 300,
     "deviceType": "Computer"
   }
   ```
2. **Verify response** includes device information
3. **Check database** for WatchProgress record

### **Step 3: Test Watch Progress Retrieval**
1. **Get movie progress**: `GET /api/watchprogress/movie/1`
2. **Verify**: Returns current position (300 seconds)
3. **Check**: Device information is properly stored

### **Step 4: Test Cross-Session Persistence**
1. **Logout and login again** (new session)
2. **Get movie progress**: `GET /api/watchprogress/movie/1`
3. **Verify**: Still returns saved progress from previous session

## 📊 **Database Check Queries**

### **Check User Sessions**
```sql
SELECT 
    "Id", "UserId", "DeviceId", "DeviceName", "SessionToken", 
    "IsActive", "LoginTime", "LastActivity"
FROM "UserSessions" 
WHERE "UserId" = (SELECT "Id" FROM "Users" WHERE "Email" = 'nitesh@example.com')
ORDER BY "LoginTime" DESC;
```

### **Check Watch Progress**
```sql
SELECT 
    w."Id", w."UserId", w."MovieId", w."CurrentPositionInSeconds",
    w."LastWatchedDevice", w."LastWatchedDeviceId", w."LastWatchedDeviceName",
    w."LastWatchedAt", w."ProgressPercentage", m."Title"
FROM "WatchProgresses" w
JOIN "Movies" m ON w."MovieId" = m."Id"
WHERE w."UserId" = (SELECT "Id" FROM "Users" WHERE "Email" = 'nitesh@example.com');
```

## 🔧 **API Test Requests**

### **1. Debug Session Information**
```http
GET /api/watchprogress/debug/session
Authorization: Bearer {jwt_token}
```

**Expected Response:**
```json
{
  "currentSessionToken": "guid-string",
  "userId": 2,
  "allSessions": [
    {
      "id": 1,
      "deviceId": "device-fingerprint",
      "deviceName": "Nitesh's Computer",
      "sessionToken": "guid-string",
      "isActive": true,
      "loginTime": "2025-11-18T...",
      "lastActivity": "2025-11-18T..."
    }
  ],
  "claims": [
    {"type": "nameidentifier", "value": "2"},
    {"type": "session_token", "value": "guid-string"}
  ]
}
```

### **2. Update Watch Progress**
```http
POST /api/watchprogress
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "movieId": 1,
  "currentPositionInSeconds": 300,
  "deviceType": "Computer"
}
```

**Expected Response:**
```json
{
  "id": 1,
  "userId": 2,
  "movieId": 1,
  "currentPositionInSeconds": 300,
  "lastWatchedDevice": "Computer",
  "lastWatchedDeviceId": "device-fingerprint",
  "lastWatchedDeviceName": "Nitesh's Computer",
  "lastWatchedAt": "2025-11-18T13:45:00Z",
  "isCompleted": false,
  "progressPercentage": 3.5
}
```

### **3. Get Watch Progress**
```http
GET /api/watchprogress/movie/1
Authorization: Bearer {jwt_token}
```

**Expected Response:**
```json
{
  "id": 1,
  "userId": 2,
  "movieId": 1,
  "currentPositionInSeconds": 300,
  "lastWatchedDevice": "Computer",
  "lastWatchedDeviceId": "device-fingerprint",
  "lastWatchedDeviceName": "Nitesh's Computer",
  "lastWatchedAt": "2025-11-18T13:45:00Z",
  "isCompleted": false,
  "progressPercentage": 3.5
}
```

## 🎯 **Expected Behavior After Fix**

### **Scenario: Resume Movie**
1. **Login**: Nitesh logs in as `nitesh@example.com`
2. **Start Movie**: Begins watching "The Shawshank Redemption"
3. **Watch 5 minutes**: Progress saved to database with device info
4. **Logout/Close Browser**
5. **Login Again**: Same or different device
6. **Open Movie**: Should show "Resume from 5:00" option
7. **Click Resume**: Video starts at 5-minute mark

### **Multi-Device Sync**
- **Computer 1**: Watch 5 minutes
- **Computer 2**: Shows "Resume from 5:00 (last watched on Computer 1)"
- **Computer 3**: Same resume option with device context

## 🛠 **Fixed Components**

### **1. AuthController.cs**
- ✅ Session tokens now generated for both new and existing sessions
- ✅ Enhanced session token management

### **2. WatchProgressController.cs**  
- ✅ Improved session token extraction from JWT
- ✅ Fallback to most recent active session
- ✅ Enhanced error reporting with debug information
- ✅ Added debug endpoint for session troubleshooting

### **3. WatchProgressDto.cs**
- ✅ Added device tracking fields
- ✅ Enhanced response information

## 📝 **Next Steps**

1. **Test the complete flow** with real user scenarios
2. **Verify multi-device synchronization** works correctly  
3. **Check UI integration** to ensure resume functionality works
4. **Implement periodic progress saving** during video playback
5. **Add progress indicators** in the UI

---

**Status**: Ready for testing 🧪