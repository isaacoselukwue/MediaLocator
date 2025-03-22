window.addEventListener('error', function (e) {
    if (e.target && (e.target.tagName === 'AUDIO' || e.target.tagName === 'SOURCE')) {
        console.error('Audio element error:', e.target.error);
        console.error('Audio src:', e.target.src);
    }
}, true);

window.downloadFile = function (url, fileName) {
    const isMp32 = url.includes('mp32');
    
    fetch(url)
        .then(response => response.blob())
        .then(blob => {
            if (isMp32) {
                blob = new Blob([blob], { type: 'audio/mpeg' });
                
                if (fileName.endsWith('.mp32')) {
                    fileName = fileName.replace('.mp32', '.mp3');
                }
            }
            
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = fileName || "download";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(link.href);
        })
        .catch(error => console.error("Download error:", error));
};

window.playAudio = function (audioElement) {
    if (!audioElement) return;

    try {
        const sources = audioElement.getElementsByTagName('source');
        if (sources.length > 0) {
            for (const source of sources) {
                if (source.src && source.src.includes('mp32')) {
                    source.type = 'audio/mpeg';
                }
            }
        }

        const playPromise = audioElement.play();

        if (playPromise !== undefined) {
            playPromise
                .then(_ => {
                    console.log("Audio playback started successfully");
                })
                .catch(error => {
                    console.warn("Audio playback issue:", error);

                    const playButton = document.createElement("div");
                    playButton.className = "manual-play-button";
                    playButton.innerHTML = "<i class='bi bi-play-fill'></i> Click to play audio";
                    playButton.style.cursor = "pointer";
                    playButton.style.padding = "10px";
                    playButton.style.backgroundColor = "#f0f0f0";
                    playButton.style.borderRadius = "4px";
                    playButton.style.margin = "10px 0";
                    playButton.style.textAlign = "center";

                    playButton.addEventListener("click", function () {
                        audioElement.play()
                            .then(_ => {
                                playButton.remove();
                            })
                            .catch(e => {
                                console.error("Still cannot play audio:", e);
                                playButton.innerHTML = "Cannot play this audio due to security restrictions. Try downloading it.";
                                playButton.style.backgroundColor = "#ffecec";
                                playButton.style.color = "#d00000";
                            });
                    });

                    if (audioElement.parentNode) {
                        audioElement.parentNode.insertBefore(playButton, audioElement);
                    }
                });
        }
    } catch (e) {
        console.error("Error trying to play audio:", e);
    }
};

window.getAudioDuration = function (audioElement) {
    return new Promise((resolve) => {
        if (!audioElement) {
            resolve(0);
            return;
        }

        if (audioElement.duration && !isNaN(audioElement.duration)) {
            resolve(audioElement.duration);
            return;
        }

        const onLoadedMetadata = () => {
            if (audioElement.duration && !isNaN(audioElement.duration)) {
                resolve(audioElement.duration);
            } else {
                resolve(0);
            }
            audioElement.removeEventListener('loadedmetadata', onLoadedMetadata);
        };

        const onError = () => {
            console.error("Error loading audio");
            resolve(0);
            audioElement.removeEventListener('error', onError);
        };

        audioElement.addEventListener('loadedmetadata', onLoadedMetadata);
        audioElement.addEventListener('error', onError);

        if (audioElement.readyState >= 2) {
            onLoadedMetadata();
        }
    });
};

window.showModalDebug = function() {
    console.log("Modal debugging activated");
    const modals = document.querySelectorAll('.modal');
    const backdrops = document.querySelectorAll('.modal-backdrop');
    
    console.log(`Found ${modals.length} modals, ${backdrops.length} backdrops`);
    
    modals.forEach((modal, i) => {
        console.log(`Modal ${i}: display=${getComputedStyle(modal).display}, visibility=${getComputedStyle(modal).visibility}, zIndex=${getComputedStyle(modal).zIndex}`);
    });
};

// Store the reference and timer IDs globally
window.tokenRefreshData = {
    dotnetHelper: null,
    intervalId: null,
    timeoutId: null,
    isActive: false
};

window.setupTokenRefreshHeartbeat = function(dotnetHelper) {
    clearTokenRefreshHeartbeat();
    
    window.tokenRefreshData.dotnetHelper = dotnetHelper;
    window.tokenRefreshData.isActive = true;
    
    function checkTokenRefresh() {
        if (window.tokenRefreshData.dotnetHelper && window.tokenRefreshData.isActive) {
            try {
                window.tokenRefreshData.dotnetHelper.invokeMethodAsync('RefreshTokenIfNeeded')
                    .catch(error => {
                        console.warn("Token refresh failed, clearing heartbeat:", error);
                        if (error.toString().includes("DotNetObjectReference")) {
                            clearTokenRefreshHeartbeat();
                        }
                    });
            } catch (e) {
                console.error("Error in token refresh:", e);
                clearTokenRefreshHeartbeat();
            }
        }
    }
    
    function resetActivityTimer() {
        if (window.tokenRefreshData.timeoutId) {
            clearTimeout(window.tokenRefreshData.timeoutId);
        }
        
        if (window.tokenRefreshData.isActive) {
            window.tokenRefreshData.timeoutId = setTimeout(checkTokenRefresh, 2 * 60 * 1000);
        }
    }
    
    window.tokenRefreshData.intervalId = setInterval(checkTokenRefresh, 5 * 60 * 1000);
    
    const activityEvents = ['mousedown', 'mousemove', 'keydown', 'scroll', 'touchstart'];
    activityEvents.forEach(function(eventName) {
        document.addEventListener(eventName, resetActivityTimer, true);
    });
    
    resetActivityTimer();
    
    console.log("Token refresh heartbeat initialized");
};

window.clearTokenRefreshHeartbeat = function() {
    console.log("Clearing token refresh heartbeat");
    
    if (window.tokenRefreshData.intervalId) {
        clearInterval(window.tokenRefreshData.intervalId);
        window.tokenRefreshData.intervalId = null;
    }
    
    if (window.tokenRefreshData.timeoutId) {
        clearTimeout(window.tokenRefreshData.timeoutId);
        window.tokenRefreshData.timeoutId = null;
    }
    
    window.tokenRefreshData.isActive = false;
    window.tokenRefreshData.dotnetHelper = null;
};

window.clearAllTimers = function() {
    clearTokenRefreshHeartbeat();
};