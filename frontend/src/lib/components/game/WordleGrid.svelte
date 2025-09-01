<script lang="ts">
  import { getGame } from "../../game/game.svelte";
  import { BlockState, GameState, type Game } from "../../game/game.types";
  import { IsGameOver } from "../../shared/utils";
  import Countdown from "./Countdown.svelte";

  let game: Game = $derived(getGame());

  const cellStatusClasses: Record<BlockState, string> = {
    [BlockState.Correct]:
      "bg-green-500 border-green-500 text-white animate-flip",
    [BlockState.Present]:
      "bg-yellow-500 border-yellow-500 text-white animate-flip",
    [BlockState.Absent]: "bg-gray-600 border-gray-600 text-white animate-flip",
  };

  function getCellClasses(letter: string, row: number, col: number): string {
    const baseClasses =
      "w-14 h-14 border-2 rounded-md flex items-center justify-center font-bold text-xl transition-all duration-300";

    if (row < game.Round) {
      const status = game.Board[row].state[col];

      return `${baseClasses} ${cellStatusClasses[status]}`;
    }

    return `${baseClasses} border-gray-700 text-white/60`;
  }

  function getAnimationDelay(row: number, col: number): string {
    if (row === game.Round - 1) {
      return `${col * 150}ms`;
    }
    return "0ms";
  }
</script>

<div
  class="bg-white/10 backdrop-blur-lg rounded-2xl p-8 border border-white/20 shadow-2xl"
>
  <div class="grid grid-rows-6 gap-2 w-fit mx-auto">
    {#each Array(6) as _, rowIndex}
      <div class="grid grid-cols-5 gap-2">
        {#each Array(5) as _, colIndex}
          {@const letter =
            game.Round > 0 ? game.Board[rowIndex]?.Letters[colIndex] : ""}
          <div
            class={getCellClasses(letter, rowIndex, colIndex)}
            style="animation-delay: {getAnimationDelay(rowIndex, colIndex)}"
          >
            {letter}
          </div>
        {/each}
      </div>
    {/each}
  </div>

  {#if game.State === GameState.Won}
    <div
      class="text-center mt-6 p-4 bg-green-500/20 rounded-lg border border-green-500/30"
    >
      <p class="text-green-400 font-bold text-xl">🎉 Congratulations!</p>
      <p class="text-green-300 mt-1">
        The crowd guessed it in {game.Round} tries!
      </p>
      <p class="text-green-300 mt -1">Next game in: 
        <Countdown
          targetDate={new Date(Date.now() + game.TimeToNextGame * 1000)}
          positive={true}
          behindText="s remaining"
          finishText="soon"
        />
      </p>
    </div>
  {:else if game.State === GameState.Lost}
    <div
      class="text-center mt-6 p-4 bg-red-500/20 rounded-lg border border-red-500/30"
    >
      <p class="text-red-400 font-bold text-xl">💔 Game Over</p>
      <p class="text-red-300 mt-1">
        The word was: <span class="font-bold">{game.SelectedWord}</span>
      </p>
      <p class="text-red-300 mt -1">Next game in: 
        <Countdown
          targetDate={new Date(Date.now() + game.TimeToNextGame * 1000)}
          positive={true}
          behindText="s remaining"
          finishText="soon"
        />
      </p>
    </div>
  {/if}
  
</div>

<style>
  @keyframes flip {
    0% {
      transform: rotateY(0);
    }
    50% {
      transform: rotateY(90deg);
    }
    100% {
      transform: rotateY(0);
    }
  }

  .animate-flip {
    animation: flip 0.6s ease-in-out;
  }
</style>
