// asel-layout.js
// Place in: AselDevBlazor.Web/wwwroot/js/asel-layout.js
// Reference in index.html / _Host.cshtml:
//   <script src="js/asel-layout.js"></script>

window.aselLayout = (() => {
    const MOBILE_BREAKPOINT = 960;
    let dotNetRef   = null;
    let resizeTimer = null;

    function isMobile() {
        return window.innerWidth <= MOBILE_BREAKPOINT;
    }

    function notifyBlazor() {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('SetMobileMode', isMobile());
        }
    }

    function onResize() {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(notifyBlazor, 80);
    }

    return {
        init(ref) {
            dotNetRef = ref;
            window.addEventListener('resize', onResize);
            // Fire immediately so Blazor knows the initial state
            notifyBlazor();
        },
        dispose() {
            window.removeEventListener('resize', onResize);
            dotNetRef = null;
        }
    };
})();



// Token storage helpers
window.aselAuth = {
    setToken: (token) => localStorage.setItem('asel_jwt', token),
    getToken: () => localStorage.getItem('asel_jwt'),
    removeToken: () => localStorage.removeItem('asel_jwt')
};
