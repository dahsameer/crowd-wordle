const isLocal = ["localhost", "127.0.0.1"].includes(window.location.hostname);
const HOST = isLocal ? "localhost:5240" : window.location.host;

function IS_TLS() {
	return window.location.protocol === "https:";
}

export const HTTP_URL = isLocal ? `http://${HOST}/api` : `/api`;
const WS_URL = `${IS_TLS() ? "wss" : "ws"}://${HOST}/api/ws`;

export function GET_WS_URL(token: string) {
	return `${WS_URL}?token=${token}`;
}