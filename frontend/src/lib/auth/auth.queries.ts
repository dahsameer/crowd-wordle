import { createQuery, createMutation, useQueryClient } from '@tanstack/svelte-query';
import { fetchToken } from '../shared/api';
import { getStoredToken, storeToken, clearStoredToken } from './token.svelte';

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

export function createRefreshTokenMutation() {
	const queryClient = useQueryClient();

	return createMutation({
		mutationFn: async () => {
			const storedToken = getStoredToken();
			const token = await fetchToken(storedToken);
			storeToken(token);
			return token;
		},
		onSuccess: (newToken) => {
			queryClient.setQueryData(['token'], newToken);
		},
		onError: (error) => {
			console.error('Failed to refresh token:', error);
			clearStoredToken();
		},
	});
}