<script lang="ts">
  import { createQuery } from "@tanstack/svelte-query";
  import { getTagline } from "../../shared/api";

  const query = createQuery({
    queryKey: ["tagline"],
    queryFn: async () => {
      const tagline = await getTagline();
      return tagline;
    },
    refetchInterval: 10 * 60 * 1000,
    retry: 2,
	refetchOnWindowFocus: false,
    retryDelay: (attemptIndex) => Math.min(2000 * 2 ** attemptIndex, 30000),
  });
</script>

<div class="hidden md:block text-sm text-gray-300">
	{#if $query.isLoading}
		Loading...
	{:else if $query.isError}
		Error loading tagline
	{:else}
		{$query.data}
	{/if}
</div>
