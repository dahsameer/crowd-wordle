<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import { getWords, setWords } from "../../stream/wordStream.svelte";
  import { getGame } from "../../game/game.svelte";
  import { GameState } from "../../game/game.types";

  let { className = "" } = $props();

  const incomingWords = $derived(getWords());
  const game = $derived(getGame());

  let currentTime = $state(Date.now());

  let cleanupInterval: number;
  let animationInterval: number;

  onMount(() => {
    animationInterval = setInterval(() => {
      currentTime = Date.now();
    }, 50);

    cleanupInterval = setInterval(() => {
      const newWords = incomingWords.filter(
        (word) => Date.now() - word.timestamp < 6000
      );
      setWords(newWords);
    }, 1000);
  });

  onDestroy(() => {
    if (cleanupInterval) clearInterval(cleanupInterval);
    if (animationInterval) clearInterval(animationInterval);
  });
</script>

<div
  class="card relative overflow-hidden bg-gradient-card backdrop-blur-sm border-border/50 aspect-square {className}"
>
  <div class="absolute inset-0 bg-gradient-glow opacity-30"></div>

  <div class="relative h-full flex flex-col">
    <div class="mb-6">
      <h2 class="text-2xl font-bold text-foreground mb-2">Live Vote Feed</h2>
      <span class="font-bold text-foreground text-gray-400"
        >some recent votes</span
      >
    </div>

    <div class="flex-1 mb-6">
      <div
        class="relative h-full overflow-hidden rounded-lg border border-border/30 bg-muted/20"
      >
        {#if game.State == GameState.VotingInProgress}
          {#each incomingWords as word, index (word.id)}
            {@const age = currentTime - word.timestamp}
            {@const opacity = Math.max(1 - age / 8000)}
            {@const fallSpeed = 0.02}
            {@const yPosition = Math.min(95, age * fallSpeed)}

            <div
              class="absolute transition-opacity duration-300"
              class:text-orange-500={word.myWord}
              style:left="{word.x}%"
              style:top="{yPosition}%"
              style:opacity
              style:transform="translate(-50%, -50%)"
            >
              <div
                class="px-3 py-1 rounded-full bg-primary/20 border border-primary/30 text-primary text-sm font-mono shadow-sm"
              >
                {word.word}
              </div>
            </div>
          {/each}
        {/if}

        <div class="absolute inset-0 opacity-5">
          <div
            class="w-full h-full animate-drift"
            style:background-image="linear-gradient(90deg, transparent 0%, currentColor 50%, transparent 100%)"
            style:background-size="200px 1px"
            style:background-repeat="repeat-x"
          ></div>
        </div>
      </div>
    </div>
  </div>
</div>

<style>
  @keyframes drift {
    0% {
      background-position-x: 0%;
    }
    100% {
      background-position-x: 200px;
    }
  }

  .animate-drift {
    animation: drift 20s linear infinite;
  }
</style>
