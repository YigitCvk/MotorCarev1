window.motorCareStorage = {
    setTokens: function (accessKey, accessToken, refreshKey, refreshToken) {
        localStorage.setItem(accessKey, accessToken);
        localStorage.setItem(refreshKey, refreshToken);
    },
    get: function (key) {
        return localStorage.getItem(key);
    },
    getOrCreate: function (key) {
        var value = localStorage.getItem(key);
        if (value) {
            return value;
        }

        value = (window.crypto && window.crypto.randomUUID)
            ? window.crypto.randomUUID()
            : Math.random().toString(36).slice(2) + Date.now().toString(36);

        localStorage.setItem(key, value);
        return value;
    },
    clearTokens: function (accessKey, refreshKey) {
        localStorage.removeItem(accessKey);
        localStorage.removeItem(refreshKey);
    }
};
