<script lang="ts">
  import { getGame, getLiveData } from "../../game/game.svelte";
  import { GameState } from "../../game/game.types";
  import { IsGameOver } from "../../shared/utils";
  import Countdown from "../game/Countdown.svelte";

  let game = $derived(getGame());
  let timeToVoteEnd = $derived(game.TimeToNextGame);
  let liveData = $derived(getLiveData());
  let suggestions = $derived(liveData.topWords);
</script>

<div
  class="bg-white/10 backdrop-blur-lg rounded-xl p-4 border border-white/20 shadow-xl"
>
  <div class="flex items-center justify-between mb-3">
    <h3 class="text-lg font-bold text-white">Current Rankings</h3>
    {#if game.State == GameState.WaitingForVote}
      <div class="text-sm text-gray-300">Waiting for first vote...</div>
    {/if}
  </div>

  {#if game.State == GameState.VotingInProgress || IsGameOver(game.State)}
    <div class="text-sm text-gray-300 mb-3">
      {liveData.totalVotes} vote {liveData.totalVotes === 1 ? "•" : "s •"}
      <Countdown
        targetDate={new Date(Date.now() + timeToVoteEnd * 1000)}
        positive={false}
        behindText="s left"
        finishText="playing round with : {suggestions[0].Word}"
      />
    </div>
  {/if}
  {#if suggestions.length === 0}
    <div class="text-center py-4">
      <p class="text-gray-400">No suggestions yet</p>
      <p class="text-gray-500 text-sm">Be the first to suggest a word!</p>
    </div>
  {:else}
    <div class="space-y-2">
      {#each suggestions as suggestion, index}
        <div
          class="relative overflow-hidden rounded-lg bg-white/5 border border-white/10"
        >
          <div
            class="absolute inset-y-0 left-0 bg-purple-500/25 transition-all duration-500"
            style="width: {suggestion.Percentage}%"
          ></div>

          <div class="relative px-3 py-2 flex items-center justify-between">
            <div class="flex items-center gap-3">
              <span
                class="w-5 h-5 rounded-full bg-yellow-400 text-xs font-bold text-black flex items-center justify-center"
              >
                {index + 1}
              </span>
              <span class="font-bold text-white">{suggestion.Word}</span>
            </div>
            <div class="text-right">
              <div class="font-bold text-white text-sm">{suggestion.Count}</div>
              <div class="text-xs text-gray-400">votes</div>
            </div>
          </div>
        </div>
      {/each}
    </div>

    {#if suggestions.length > 0}
      <div class="mt-3 flex items-center justify-center gap-2 text-sm">
        <div class="w-2 h-2 bg-green-400 rounded-full animate-pulse"></div>
        <span class="text-green-300">
          <span class="text-orange-500 font-bold">{suggestions[0].Word}</span> is
          leading
        </span>
      </div>
    {/if}
  {/if}
</div>
