<script lang="ts">
  import {
    getGame,
    getConnectionState,
    getLiveData,
  } from "../../game/game.svelte";
  import { ConnectionState, GameState } from "../../game/game.types";
  import { IsGameOver } from "../../shared/utils";
  import Countdown from "../game/Countdown.svelte";

  let game = $derived(getGame());
  let liveData = $derived(getLiveData());
  let connectionState = $derived(getConnectionState());
  let suggestions = $derived(liveData.topWords);

  let connection = $derived(connectionState == ConnectionState.Connected);
  let error = $derived.by(() => {
    if (connectionState == ConnectionState.Disconnected) {
      return "Disconnected";
    }
    return null;
  });
</script>

<div class="container mx-auto px-4">
  <div
    class="bg-black/20 backdrop-blur-sm border border-white/10 rounded-xl p-4 mb-6"
  >
    <div class="flex flex-wrap items-center justify-between gap-4 text-sm">
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-2">
          <div
            class="w-3 h-3 rounded-full transition-all duration-300"
            class:bg-green-400={connection}
            class:animate-pulse={connection}
            class:bg-red-400={!connection}
          ></div>
          <span
            class="font-medium"
            class:text-green-300={connection}
            class:text-red-300={!connection}
          >
            {connection ? "Connected" : "Disconnected"}
          </span>
        </div>

        <div class="text-gray-400">
          <span
            >🎯 Round <span class="font-bold"
              >{IsGameOver(game.State) ? game.Round : game.Round + 1}</span
            ></span
          >
        </div>
      </div>

      {#if error}
        <div
          class="flex items-center gap-2 text-red-300 bg-red-500/20 px-3 py-1 rounded-lg border border-red-500/30"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <span class="text-sm font-medium">Error: {error}</span>
        </div>
      {/if}
      <div class="flex items-center gap-6">
        <div class="flex items-center gap-2">
          <svg
            class="w-4 h-4 text-blue-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"
            />
          </svg>
          <span class="text-blue-300 font-medium">{liveData.userCount}</span>
          <span class="text-gray-400 hidden sm:inline">
            player{liveData.userCount !== 1 ? "s" : ""} online
          </span>
        </div>
      </div>
    </div>
  </div>
</div>
