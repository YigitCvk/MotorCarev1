window.motorCareStorage = {
    setTokens: function (accessKey, accessToken, refreshKey, refreshToken) {
        localStorage.setItem(accessKey, accessToken);
        localStorage.setItem(refreshKey, refreshToken);
    },
    get: function (key) {
        return localStorage.getItem(key);
    },
    clearTokens: function (accessKey, refreshKey) {
        localStorage.removeItem(accessKey);
        localStorage.removeItem(refreshKey);
    }
};
