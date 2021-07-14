function log() {
    const resultElement = document.getElementById("results");
    resultElement.innerText = "";
    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = `Error: ${msg.message}`;
        } else if (typeof msg !== "string") {
            msg = JSON.stringify(msg, null, 2);
        }
        resultElement.innerHTML += msg + "\r\n";
    });
}

document.getElementById("login").addEventListener("click", login, false);
document.getElementById("api").addEventListener("click", api, false);
document.getElementById("logout").addEventListener("click", logout, false);

const config = {
    authority: "http://localhost:5000",
    client_id: "js",
    redirect_uri: "http://localhost:5003/callback.html",
    response_type: "code",
    scope: "openid profile api1",
    post_logout_redirect_uri: "http://localhost:5003/index.html"
}

const mgr = new Oidc.UserManager(config);

function CheckUserLoginStatus() {
    mgr.getUser().then(function (user) {
        if (user) {
            log(user);
        } else {
            log("User not logged in");
        }
    });
}

function login() {
    mgr.signinRedirect();
}

function api() {
    mgr.getUser().then(user => {
        const url = "http://localhost:5001/identity";
        const xhr = new XMLHttpRequest();
        xhr.open("GET", url);
        xhr.onload = function () {
            log(xhr.status, JSON.parse(xhr.responseText));
        }
        xhr.setRequestHeader("Authorization", `Bearer ${user.access_token}`);
        xhr.send();
    });
}

function logout() {
    mgr.signoutRedirect();
}

