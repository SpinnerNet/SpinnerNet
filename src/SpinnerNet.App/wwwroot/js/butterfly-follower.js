// Butterfly cursor follower functionality
let dotNetHelper = null;
let mousePosition = { x: 0, y: 0 };
let isTracking = false;
let updateTimer = null;

// Initialize the butterfly follower
export function initializeButterflies(dotNetRef) {
    dotNetHelper = dotNetRef;
    
    // Start tracking mouse movement
    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseenter', handleMouseEnter);
    document.addEventListener('mouseleave', handleMouseLeave);
    
    isTracking = true;
}

// Handle mouse movement
function handleMouseMove(event) {
    if (!isTracking || !dotNetHelper) return;
    
    mousePosition.x = event.clientX;
    mousePosition.y = event.clientY;
    
    // Throttle updates to avoid overwhelming the component
    if (updateTimer) {
        clearTimeout(updateTimer);
    }
    
    updateTimer = setTimeout(() => {
        dotNetHelper.invokeMethodAsync('UpdateCursorPosition', mousePosition.x, mousePosition.y);
    }, 100);
}

// Handle mouse entering the window
function handleMouseEnter(event) {
    isTracking = true;
}

// Handle mouse leaving the window
function handleMouseLeave(event) {
    isTracking = false;
    
    // Move butterflies to idle positions when cursor leaves
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('UpdateCursorPosition', -100, -100);
    }
}

// Cleanup function
export function cleanup() {
    isTracking = false;
    
    if (updateTimer) {
        clearTimeout(updateTimer);
        updateTimer = null;
    }
    
    document.removeEventListener('mousemove', handleMouseMove);
    document.removeEventListener('mouseenter', handleMouseEnter);
    document.removeEventListener('mouseleave', handleMouseLeave);
    
    dotNetHelper = null;
}

// Touch support for mobile devices
document.addEventListener('touchmove', function(event) {
    if (!isTracking || !dotNetHelper) return;
    
    const touch = event.touches[0];
    if (touch) {
        mousePosition.x = touch.clientX;
        mousePosition.y = touch.clientY;
        
        if (updateTimer) {
            clearTimeout(updateTimer);
        }
        
        updateTimer = setTimeout(() => {
            dotNetHelper.invokeMethodAsync('UpdateCursorPosition', mousePosition.x, mousePosition.y);
        }, 150); // Slightly longer delay for touch
    }
});

// Prevent scrolling when touching butterflies
document.addEventListener('touchstart', function(event) {
    // Only prevent default if touching in butterfly areas
    const target = event.target.closest('.butterfly');
    if (target) {
        event.preventDefault();
    }
}, { passive: false });