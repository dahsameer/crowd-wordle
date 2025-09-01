const TOKEN_STORAGE_KEY = 'crowordle';

export function getStoredToken(): string | null {
	return localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function storeToken(token: string): void {
	localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearStoredToken(): void {
	localStorage.removeItem(TOKEN_STORAGE_KEY);
}