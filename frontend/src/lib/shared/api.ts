import { HTTP_URL } from "./constants";

export async function fetchToken(token: string | null): Promise<string> {
	const response = await fetch(`${HTTP_URL}/auth`, {
		method: 'POST',
		body: JSON.stringify({ token }),
		headers: {
			"Content-Type": "application/json"
		}
	});
	if (!response.ok) {
		throw new Error('Failed to fetch token');
	}
	return response.text();
}

export async function getTagline(): Promise<string> {
	const response = await fetch(`${HTTP_URL}/tagline`, {
		method: 'GET'
	});
	if (!response.ok) {
		return "Error in getting tagline";
	}
	return await response.text();
}