/**
 * api.js – Helper wrapper cho fetch calls tới fe-api
 * Tính năng:
 * - Tự động đính kèm credentials (session cookie)
 * - Proactive token refresh: decode JWT từ meta tag, refresh nếu còn < 2 phút
 * - Reactive 401: nếu server trả 401, gọi /fe-api/auth/refresh rồi retry 1 lần
 */
window.api = (() => {
    const baseUrl = document.querySelector('meta[name="fe-api"]')?.content ?? "";

    // ── JWT decode (không verify, chỉ đọc exp) ─────────────────────────────
    function decodeJwtExp(token) {
        try {
            const payload = token.split(".")[1];
            const decoded = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
            return decoded.exp ?? null;   // seconds since epoch
        } catch {
            return null;
        }
    }

    // Kiểm tra token còn hạn dưới 2 phút không (proactive buffer)
    function isTokenExpiringSoon(token) {
        const exp = decodeJwtExp(token);
        if (!exp) return false;
        const nowSec = Math.floor(Date.now() / 1000);
        return (exp - nowSec) < 120;     // < 2 phút
    }

    // ── Proactive refresh ───────────────────────────────────────────────────
    let _refreshingPromise = null;   // singleton để tránh race condition

    async function proactiveRefresh() {
        if (_refreshingPromise) return _refreshingPromise;

        _refreshingPromise = (async () => {
            try {
                const res = await fetch(`${baseUrl}/fe-api/auth/refresh`, {
                    method: "POST",
                    credentials: "include"
                });
                if (!res.ok) {
                    // refresh token hết hạn → redirect login
                    window.location.href = "/";
                }
            } catch {
                // silent fail
            } finally {
                _refreshingPromise = null;
            }
        })();

        return _refreshingPromise;
    }

    // ── Core request ────────────────────────────────────────────────────────
    async function request(path, options = {}) {
        // Options: enableCache (default true for GET), ...
        const method = options.method || "GET";
        const enableCache = options.enableCache ?? (method === "GET");
        
        // Key generation for cache
        const cacheKey = enableCache 
            ? `funews_cache_${method}_${path}_${options.body || ""}` 
            : null;

        // Proactive token refresh (meta tag logic)
        const tokenMeta = document.querySelector('meta[name="token-exp"]');
        if (tokenMeta) {
            const expSec = parseInt(tokenMeta.content, 10);
            const nowSec = Math.floor(Date.now() / 1000);
            if (!isNaN(expSec) && (expSec - nowSec) < 120) {
                await proactiveRefresh();
            }
        }

        try {
            const res = await fetch(`${baseUrl}${path}`, {
                ...options,
                headers: {
                    "Content-Type": "application/json",
                    ...(options.headers || {})
                },
                credentials: "include"
            });

            // Reactive: 401 → refresh → retry
            if (res.status === 401) {
                await proactiveRefresh();

                // retry
                const retryRes = await fetch(`${baseUrl}${path}`, {
                    ...options,
                    headers: {
                        "Content-Type": "application/json",
                        ...(options.headers || {})
                    },
                    credentials: "include"
                });

                if (retryRes.status === 401) {
                    window.location.href = "/";   // hết hạn hẳn → login lại
                    throw new Error("Session expired.");
                }

                const data = await parseResponse(retryRes);
                if (enableCache && data && data.success !== false) {
                     try { localStorage.setItem(cacheKey, JSON.stringify({ ts: Date.now(), data })); } catch {}
                }
                return data;
            }

            const data = await parseResponse(res);
            
            // Success -> Cache it
            if (enableCache && res.ok && data && data.success !== false) {
                try { localStorage.setItem(cacheKey, JSON.stringify({ ts: Date.now(), data })); } catch {}
            }
            
            return data;

        } catch (err) {
            // Network error / Offline
            if (enableCache && cacheKey) {
                const cached = localStorage.getItem(cacheKey);
                if (cached) {
                    try {
                        const parsed = JSON.parse(cached);
                        console.warn(`[Offline Mode] Serving cached data for ${path}`);
                        // Notify user visually if needed? (Banner is handled by site.js)
                        return parsed.data;
                    } catch {}
                }
            }
            throw err;
        }
    }

    async function parseResponse(res) {
        const text = await res.text();
        let data = null;
        try {
            data = text ? JSON.parse(text) : null;
        } catch {
            data = text;
        }

        if (data && data.success === false) {
            const msg = data.message || data.Message || "Request failed";
            throw new Error(msg);
        }

        if (!res.ok && (!data || data.success === undefined)) {
            const msg = (data && (data.message || data.Message)) || res.statusText;
            throw new Error(msg);
        }

        return data;
    }

    return {
        get:  (path)        => request(path, { method: "GET" }),
        post: (path, body)  => request(path, { method: "POST",  body: JSON.stringify(body) }),
        put:  (path, body)  => request(path, { method: "PUT",   body: JSON.stringify(body) }),
        del:  (path)        => request(path, { method: "DELETE" })
    };
})();