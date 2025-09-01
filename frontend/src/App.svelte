<script lang="ts">
  import WordleGrid from "./lib/components/game/WordleGrid.svelte";
  import WordInput from "./lib/components/game/WordInput.svelte";
  import SuggestionStream from "./lib/components/suggestions/SuggestionStream.svelte";
  import StatusBar from "./lib/components/layout/StatusBar.svelte";
  import InfoModal from "./lib/components/layout/InfoModal.svelte";
  import TagLine from "./lib/components/layout/TagLine.svelte";
  import { onDestroy } from "svelte";
  import { initializeWebSocket } from "./lib/game/websocket/websocket";
  import { getConnectionState } from "./lib/game/game.svelte";
  import { ConnectionState } from "./lib/game/game.types";
  import { createTokenQuery } from "./lib/auth/auth.queries";
  import { encodeToPackedNum } from "./lib/game/websocket/encoding";
  import TopVotes from "./lib/components/suggestions/TopVotes.svelte";
  import Toast from "./lib/components/layout/Toast.svelte";
  import { setMyWord } from "./lib/stream/wordStream.svelte";

  let showInfo = $state(false);
  let socket: WebSocket | null = null;
  let connectionState = $derived(getConnectionState());

  const tokenQuery = createTokenQuery();
  $effect(() => {
    if ($tokenQuery.data && $tokenQuery.isSuccess) {
      if (socket != null) {
        socket.close();
      }
      socket = initializeWebSocket($tokenQuery.data);
    }
  });

  onDestroy(() => {
    if (socket != null && connectionState === ConnectionState.Connected) {
      socket.close();
    }
  });

  function handleWordSubmit(word: string) {
    setMyWord(word.toUpperCase());
    const encodedString = encodeToPackedNum(word);
    const buffer = new ArrayBuffer(4);
    const view = new DataView(buffer);
    view.setUint32(0, encodedString, false);
    socket?.send(buffer);
  }
</script>

<main class="min-h-screen bg-black text-white">
  <header class="container mx-auto px-4 py-6">
    <div class="flex items-center justify-between">
      <div class="flex items-center gap-4">
        <h1
          class="text-4xl font-bold bg-orange-500 bg-clip-text text-transparent"
        >
          Crowd Wordle
        </h1>
        <TagLine />
      </div>

      <div class="flex items-center gap-4">
        <button
          onclick={() => (showInfo = true)}
          class="p-2 bg-white/10 hover:bg-white/20 rounded-lg backdrop-blur-sm border border-white/20 transition-all duration-200 hover:scale-105 focus:outline-none focus:ring-2 focus:ring-white/30"
          aria-label="Info"
          title="Info"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="w-5 h-5"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.6"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
            role="img"
          >
            <circle cx="12" cy="12" r="10"></circle>
            <path d="M9.09 9a3 3 0 1 1 5.82 1c0 2-3 2.5-3 4"></path>
            <circle cx="12" cy="17" r="1"></circle>
          </svg>
        </button>
      </div>
    </div>
  </header>

  <StatusBar />

  <div class="container mx-auto px-4 py-8">
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2 space-y-8">
        <div class="flex justify-center">
          <WordleGrid />
        </div>

        <WordInput submit={(word: string) => handleWordSubmit(word)} />
      </div>

      <div class="space-y-6">
        <SuggestionStream />
        <TopVotes />
      </div>
    </div>
  </div>

  {#if showInfo}
    <InfoModal close={() => (showInfo = false)} />
  {/if}
  <Toast />
</main>

<style>
  :global(body) {
    font-family:
      "Inter",
      -apple-system,
      BlinkMacSystemFont,
      "Segoe UI",
      Roboto,
      sans-serif;
  }
</style>
