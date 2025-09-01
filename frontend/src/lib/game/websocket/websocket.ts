import { GET_WS_URL } from "../../shared/constants";
import { handleData } from "../datahandler";
import { setConnectionState } from "../game.svelte";
import { ConnectionState } from "../game.types";

export let socket: WebSocket | null = null;

export function initializeWebSocket(token: string): WebSocket {
	setConnectionState(ConnectionState.Connecting);

	socket = new WebSocket(GET_WS_URL(token));
	socket.binaryType = "arraybuffer";

	socket.addEventListener("open", (event) => {
		setConnectionState(ConnectionState.Connected);
	});

	socket.addEventListener("message", (event) => {
		const buffer = new Uint8Array(event.data);
		handleData(buffer);
	});

	socket.addEventListener("close", () => {
		setConnectionState(ConnectionState.Disconnected);
	});
	socket.addEventListener("error", () => {
		setConnectionState(ConnectionState.Disconnected);
	});

	return socket;
}