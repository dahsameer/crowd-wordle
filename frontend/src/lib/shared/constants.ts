const HOST: string = "localhost:5240";
function IS_TLS(): boolean {
	return window.location.protocol == "https:";
}
export const HTTP_URL = IS_TLS() ? `https://${HOST}/api` : `http://${HOST}/api`;
const WS_URL = IS_TLS() ? `wss://${HOST}/api/ws` : `ws://${HOST}/api/ws`;

export function GET_WS_URL(token: string) {
	console.log("token", token);
	return `${WS_URL}?token=${token}`;
}