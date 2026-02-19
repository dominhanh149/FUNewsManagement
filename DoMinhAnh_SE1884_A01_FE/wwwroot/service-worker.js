const CACHE_NAME = "funews-offline-v1";
const OFFLINE_URL = "/offline.html";

const ASSETS_TO_CACHE = [
    OFFLINE_URL,
    "/css/site.css",
    "/js/site.js",
    "/js/api.js",
    "/js/signalr-client.js"
    // Bootstrap CDN links are external and handled by browser disk cache mostly, 
    // but explicit caching requires opaque response handling or downloading them locally.
    // For simplicity, we rely on browser cache for CDNs or network fallback.
];

self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            console.log("[ServiceWorker] Pre-caching offline page");
            return cache.addAll(ASSETS_TO_CACHE);
        })
    );
    self.skipWaiting();
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((keyList) => {
            return Promise.all(
                keyList.map((key) => {
                    if (key !== CACHE_NAME) {
                        console.log("[ServiceWorker] Removing old cache", key);
                        return caches.delete(key);
                    }
                })
            );
        })
    );
    self.clients.claim();
});

self.addEventListener("fetch", (event) => {
    // 1. Navigation requests (HTML pages)
    if (event.request.mode === "navigate") {
        event.respondWith(
            fetch(event.request)
                .catch(() => {
                    return caches.match(OFFLINE_URL);
                })
        );
        return;
    }

    // 2. Static Assets (CSS, JS, Images from our origin)
    // We use Stale-While-Revalidate for our assets
    if (event.request.destination === "style" || 
        event.request.destination === "script" || 
        event.request.destination === "image") {
        
        event.respondWith(
            caches.match(event.request).then((cachedResponse) => {
                const fetchPromise = fetch(event.request).then((networkResponse) => {
                    // Update cache if valid
                    if (networkResponse && networkResponse.status === 200 && networkResponse.type === 'basic') {
                        const responseToCache = networkResponse.clone();
                        caches.open(CACHE_NAME).then((cache) => {
                            cache.put(event.request, responseToCache);
                        });
                    }
                    return networkResponse;
                }).catch(() => {
                    // console.log("Fetch failed for asset, keeping cache");
                });

                return cachedResponse || fetchPromise;
            })
        );
        return;
    }

    // 3. API Requests - handled by api.js or just pass through
    // We do NOT cache API calls in SW in this strategy, to keep logic in api.js 
    // or we could, but api.js logic is already complex enough.
});
