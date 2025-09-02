import { createQuery } from '@tanstack/svelte-query';
import { fetchToken } from '../shared/api';
import { getStoredToken, storeToken } from './token.svelte';

export function createTokenQuery() {
	return createQuery({
		queryKey: ['token'],
		queryFn: async () => {
			const storedToken = getStoredToken();
			const token = await fetchToken(storedToken);
			storeToken(token);
			return token;
		},
		staleTime: 365 * 24 * 60 * 60 * 1000,
		retry: 2,
		retryDelay: (attemptIndex) => Math.min(2000 * 2 ** attemptIndex, 30000),
	});
}
