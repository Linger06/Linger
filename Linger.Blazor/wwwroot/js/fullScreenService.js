// 浏览器全屏服务 JavaScript 模块
export function initFullScreenListener(dotNetHelper) {
    // 监听全屏状态变化事件
    document.addEventListener('fullscreenchange', () => {
        const isFullScreen = document.fullscreenElement !== null;
        dotNetHelper.invokeMethodAsync('OnFullScreenChange', isFullScreen);
    });
    
    // 监听webkit全屏状态变化事件（Safari）
    document.addEventListener('webkitfullscreenchange', () => {
        const isFullScreen = document.webkitFullscreenElement !== null;
        dotNetHelper.invokeMethodAsync('OnFullScreenChange', isFullScreen);
    });
    
    // 监听moz全屏状态变化事件（Firefox）
    document.addEventListener('mozfullscreenchange', () => {
        const isFullScreen = document.mozFullScreenElement !== null;
        dotNetHelper.invokeMethodAsync('OnFullScreenChange', isFullScreen);
    });
    
    // 监听MS全屏状态变化事件（旧版IE/Edge）
    document.addEventListener('MSFullscreenChange', () => {
        const isFullScreen = document.msFullscreenElement !== null;
        dotNetHelper.invokeMethodAsync('OnFullScreenChange', isFullScreen);
    });
}

// 检查当前是否处于全屏状态
export function isFullScreen() {
    return document.fullscreenElement !== null || 
           document.webkitFullscreenElement !== null || 
           document.mozFullScreenElement !== null ||
           document.msFullscreenElement !== null;
}

// 切换全屏状态
export function toggleFullScreen() {
    if (isFullScreen()) {
        exitFullScreen();
    } else {
        enterFullScreen();
    }
}

// 进入全屏模式
function enterFullScreen() {
    const docElm = document.documentElement;
    
    if (docElm.requestFullscreen) {
        docElm.requestFullscreen();
    } else if (docElm.webkitRequestFullScreen) {
        docElm.webkitRequestFullScreen();
    } else if (docElm.mozRequestFullScreen) {
        docElm.mozRequestFullScreen();
    } else if (docElm.msRequestFullscreen) {
        docElm.msRequestFullscreen();
    }
}

// 退出全屏模式
function exitFullScreen() {
    if (document.exitFullscreen) {
        document.exitFullscreen();
    } else if (document.webkitExitFullscreen) {
        document.webkitExitFullscreen();
    } else if (document.mozCancelFullScreen) {
        document.mozCancelFullScreen();
    } else if (document.msExitFullscreen) {
        document.msExitFullscreen();
    }
}