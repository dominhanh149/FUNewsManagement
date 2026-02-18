document.getElementById("btnLogin").addEventListener("click", async () => {
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    const msg = document.getElementById("msg");
    msg.innerText = "";

    try {
        const res = await api.post("/fe-api/auth/login", {
            Email: email,      // Capital E
            Password: password  // Capital P
        });

        // Save user info and refresh navbar
        if (res.user) {
            localStorage.setItem("funews_user", JSON.stringify(res.user));
        }
        
        window.location.href = "/";

    } catch (e) {
        msg.innerText = e.message || "Login failed";
    }
});
