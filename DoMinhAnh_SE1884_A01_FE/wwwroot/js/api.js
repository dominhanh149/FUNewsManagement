window.api = (() => {
    const baseUrl = document.querySelector('meta[name="fe-api"]').content;

    async function request(path, options = {}) {
        const res = await fetch(`${baseUrl}${path}`, {
            ...options,
            headers: {
                "Content-Type": "application/json",
                ...(options.headers || {})
            },
            credentials: "include"
        });

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
        get: (path) => request(path, { method: "GET" }),
        post: (path, body) => request(path, { method: "POST", body: JSON.stringify(body) }),
        put: (path, body) => request(path, { method: "PUT", body: JSON.stringify(body) }),
        del: (path) => request(path, { method: "DELETE" })
    };
})();