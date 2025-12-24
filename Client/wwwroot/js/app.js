// Device detection and identification
window.getDeviceType = function () {
    const userAgent = navigator.userAgent.toLowerCase();
    const isMobile = /mobile|android|iphone|ipod|blackberry|iemobile|opera mini/i.test(userAgent);
    const isTablet = /tablet|ipad/i.test(userAgent);
    
    if (isTablet) {
        return "Tablet";
    } else if (isMobile) {
        return "Mobile";
    } else {
        return "Computer";
    }
};

// Enhanced device information collection for offline scenarios
window.collectDeviceInfo = function () {
    const nav = navigator;
    const screen = window.screen;
    
    // Generate consistent hardware identifiers based on system characteristics
    // that should be the same across browsers on the same computer
    
    // Use a combination of system properties to generate consistent MAC address
    function generateConsistentMac() {
        // Create a consistent seed based on system characteristics
        const screenInfo = `${screen.width}x${screen.height}x${screen.colorDepth}`;
        const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        const platform = nav.platform;
        const language = nav.language;
        
        // Create a hash-like value from system properties
        let hash = 0;
        const combined = `${screenInfo}-${timeZone}-${platform}-${language}`;
        for (let i = 0; i < combined.length; i++) {
            const char = combined.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash; // Convert to 32-bit integer
        }
        
        // Convert to MAC-like format
        const macPart = Math.abs(hash).toString(16).padStart(12, '0').substring(0, 12);
        return macPart.match(/.{2}/g).join(':').toUpperCase();
    }
    
    // Generate processor ID based on system characteristics
    function generateConsistentProcessorId() {
        const cores = nav.hardwareConcurrency || 4;
        const memory = nav.deviceMemory || 8;
        const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        
        return `CPU-${cores}C${memory}G-${timeZone.replace(/\//g, '').substring(0, 8)}`;
    }
    
    // Generate machine GUID based on consistent system properties
    function generateConsistentMachineGuid() {
        const platform = nav.platform;
        const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        const screenId = `${screen.width}x${screen.height}`;
        
        // Create a deterministic GUID from system properties
        const seed = `${platform}-${timeZone}-${screenId}`;
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
            hash = ((hash << 5) - hash) + seed.charCodeAt(i);
            hash = hash & hash;
        }
        
        const guidBase = Math.abs(hash).toString(16).padStart(8, '0');
        return `${guidBase}-${guidBase.substring(0,4)}-${guidBase.substring(4,8)}-${guidBase.substring(0,4)}-${guidBase}${guidBase.substring(0,4)}`;
    }
    
    // Get device name from user prompt or use computer name
    function getDeviceName() {
        let deviceName = localStorage.getItem('GiriMovies_device_name');
        if (!deviceName) {
            deviceName = prompt("Enter a name for this computer (same name for all browsers):") || "Unknown Device";
            localStorage.setItem('GiriMovies_device_name', deviceName);
        }
        return deviceName;
    }
    
    // Generate consistent identifiers (these should be the same across browsers on the same computer)
    const macAddress = generateConsistentMac();
    const processorId = generateConsistentProcessorId();
    const machineGuid = generateConsistentMachineGuid();
    const deviceName = getDeviceName();
    
    console.log("Device Info Generated:", {
        mac: macAddress,
        processor: processorId,
        guid: machineGuid,
        name: deviceName
    });
    
    return {
        macAddress: macAddress,
        processorId: processorId,
        machineGuid: machineGuid,
        computerName: deviceName,
        userName: "User",
        osVersion: nav.platform,
        localIP: "192.168.1.100", // Simulated consistent local IP
        timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        userAgent: nav.userAgent, // This will differ between browsers but won't be used for device ID
        screenResolution: screen.width + "x" + screen.height,
        deviceName: deviceName
    };
};

// Store device name (consistent across browsers)
window.setDeviceName = function (name) {
    localStorage.setItem('GiriMovies_device_name', name);
};

// Get stored device ID
window.getStoredDeviceId = function () {
    return localStorage.getItem('giri_device_id') || '';
};

// Store device ID
window.storeDeviceId = function (deviceId) {
    localStorage.setItem('giri_device_id', deviceId);
};

// Video player helpers - simplified and robust
window.setVideoTime = function (videoElement, time) {
    console.log(`🎬 setVideoTime called with time: ${time} seconds`);
    
    if (!videoElement) {
        console.error(`❌ Video element is null or undefined`);
        return;
    }
    
    console.log(`📊 Video state: readyState=${videoElement.readyState}, currentTime=${videoElement.currentTime}, duration=${videoElement.duration}`);
    
    try {
        // Simple approach - just set the time
        videoElement.currentTime = time;
        console.log(`✅ Video currentTime set to: ${videoElement.currentTime} (requested: ${time})`);
        
        // Verify after a short delay
        setTimeout(() => {
            console.log(`🔍 Final verification: Video is at ${videoElement.currentTime}s (should be ${time}s)`);
            if (Math.abs(videoElement.currentTime - time) > 2) {
                console.warn(`⚠️ Video time mismatch! Expected: ${time}s, Actual: ${videoElement.currentTime}s`);
                
                // Try one more time
                console.log(`🔄 Trying to set time again...`);
                videoElement.currentTime = time;
            }
        }, 500);
        
    } catch (error) {
        console.error(`❌ Error setting video time: ${error.message}`);
    }
};

window.getVideoTime = function (videoElement) {
    if (videoElement) {
        console.log(`Current video time: ${videoElement.currentTime} seconds`);
        return videoElement.currentTime;
    } else {
        console.error(`❌ Video element is null when getting time`);
        return 0;
    }
};

window.playVideo = function (videoElement) {
    if (videoElement) {
        videoElement.play();
    }
};

window.pauseVideo = function (videoElement) {
    if (videoElement) {
        videoElement.pause();
    }
};
