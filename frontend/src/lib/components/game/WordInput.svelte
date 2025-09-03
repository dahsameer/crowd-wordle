<script lang="ts">
  import { getGame, getLiveData, setLiveData } from "../../game/game.svelte";
  import { BlockState, SubmitState } from "../../game/game.types";
  import { showError } from "../../shared/toast.svelte";
  import { IsGameOver } from "../../shared/utils";

  const { submit } = $props();

  type LetterStatus = "absent" | "present" | "correct" | "default";

  let game = $derived(getGame());
  let liveData = $derived(getLiveData());

  let inputValue = $state("");
  let submitState = $state<SubmitState>(SubmitState.Idle);
  let inputElement = $state<HTMLInputElement | undefined>(undefined);

  let hasAlreadyVoted = $derived(!!liveData.myVote);
  let canSubmit = $derived(
    !IsGameOver(game.State) &&
      submitState === SubmitState.Idle &&
      !hasAlreadyVoted &&
      inputValue.length === 5
  );

  $effect(() => {
    if (!hasAlreadyVoted && inputElement) {
      inputElement.focus();
    }
  });

  function validateWord(word: string): boolean {
    if (word.length !== 5) {
      showError("Word must be exactly 5 letters long");
      return false;
    }

    if (!/^[A-Za-z]+$/.test(word)) {
      showError("Word must contain only letters");
      return false;
    }

    return true;
  }

  async function handleSubmit() {
    if (!canSubmit) return;

    const word = inputValue.toUpperCase();
    if (!validateWord(word)) return;

    submitState = SubmitState.Submitting;

    try {
      await submit(word);
      inputValue = "";
      submitState = SubmitState.Success;
      setLiveData({ ...liveData, myVote: word });
      setTimeout(() => {
        submitState = SubmitState.Idle;
      }, 1000);
    } catch (err) {
      showError("Failed to submit suggestion. Please try again.");
      submitState = SubmitState.Error;

      setTimeout(() => {
        submitState = SubmitState.Idle;
      }, 1000);
    }
  }

  function handleKeydown(event: KeyboardEvent) {
    if (event.key === "Enter") {
      handleSubmit();
      return;
    }

    if (event.key.length === 1 && !/[a-zA-Z]/.test(event.key)) {
      event.preventDefault();
    }
  }

  function handleInput(event: Event) {
    const target = event.target as HTMLInputElement;
    inputValue = target.value.toUpperCase().slice(0, 5);
  }

  let letterMap: Record<string, LetterStatus> = $derived.by(() => {
    const newMap: Record<string, LetterStatus> = {};
    const statusRank: Record<LetterStatus, number> = {
      default: 0,
      absent: 1,
      present: 2,
      correct: 3,
    };

    for (const row of game.Board) {
      for (let i = 0; i < row.Letters.length; i++) {
        const char = row.Letters[i].toUpperCase();
        const state = row.state[i];
        let status: LetterStatus = "default";

        if (state === BlockState.Absent) status = "absent";
        else if (state === BlockState.Present) status = "present";
        else if (state === BlockState.Correct) status = "correct";

        if (!newMap[char] || statusRank[status] > statusRank[newMap[char]]) {
          newMap[char] = status;
        }
      }
    }
    return newMap;
  });

  const keyboardLayout = [
    ["Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"],
    ["A", "S", "D", "F", "G", "H", "J", "K", "L"],
    ["Z", "X", "C", "V", "B", "N", "M"],
  ];

  function getLetterStatus(letter: string): LetterStatus {
    return letterMap[letter.toUpperCase()] ?? "default";
  }

  // --- End of logic from the second component ---
</script>

<div
  class="bg-white/10 backdrop-blur-lg rounded-2xl border border-white/20 shadow-2xl"
>
  {#if hasAlreadyVoted}
    <div class="mb-6 p-4 bg-green-500/20 border border-green-500/30 rounded-xl">
      <div class="flex items-center justify-center gap-3">
        <p class="text-green-300 font-medium">
          Your suggestion: <span
            class="text-white font-bold text-lg tracking-wider"
            >{liveData.myVote}</span
          >
        </p>
      </div>
    </div>
  {:else}
    <div class="flex flex-col sm:flex-row gap-4 m-6">
      <div class="flex-1">
        <input
          bind:this={inputElement}
          bind:value={inputValue}
          oninput={handleInput}
          onkeydown={handleKeydown}
          placeholder="ENTER WORD..."
          maxlength="5"
          disabled={!canSubmit && submitState !== SubmitState.Idle}
          class="w-full px-6 py-4 text-2xl font-bold text-center text-white bg-white/10 border-2 border-white/20 rounded-xl placeholder-white/50 focus:outline-none focus:border-yellow-400 focus:bg-white/20 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed tracking-wider"
          class:animate-pulse={submitState === SubmitState.Submitting}
          class:border-red-400={submitState === SubmitState.Error}
          class:border-green-400={submitState === SubmitState.Success}
          autocomplete="off"
        />
      </div>

      <button
        onclick={handleSubmit}
        disabled={!canSubmit}
        class="px-8 py-4 font-bold rounded-xl transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed hover:scale-105 active:scale-95 shadow-lg min-w-[120px]"
        class:bg-orange-500={submitState === SubmitState.Idle}
        class:hover:bg-orange-400={submitState === SubmitState.Idle}
        class:bg-gray-500={submitState === SubmitState.Submitting}
        class:bg-green-500={submitState === SubmitState.Success}
        class:bg-red-500={submitState === SubmitState.Error}
        class:text-white={true}
        class:hover:shadow-xl={submitState === SubmitState.Idle}
      >
        {#if submitState === SubmitState.Submitting}
          <div class="flex items-center justify-center gap-2">
            <div
              class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"
            ></div>
            Sending
          </div>
        {:else if submitState === SubmitState.Success}
          ✓ Sent!
        {:else if submitState === SubmitState.Error}
          Try Again
        {:else}
          Suggest
        {/if}
      </button>
    </div>
  {/if}

  <div
    class="w-full flex flex-col items-center gap-2 mx-auto pb-4 select-none opacity-70"
  >
    {#each keyboardLayout as row}
      <div class="flex justify-center gap-1.5 w-full">
        {#each row as key}
          <div
            class="h-12 rounded-md font-bold text-sm flex items-center justify-center transition-colors flex-shrink"
            class:flex-1={key.length === 1}
            class:max-w-[48px]={key.length === 1}
            class:min-w-[64px]={key.length > 1}
            class:bg-gray-500={getLetterStatus(key) === "default"}
            class:bg-gray-800={getLetterStatus(key) === "absent"}
            class:text-white={getLetterStatus(key) === "absent"}
            class:bg-yellow-500={getLetterStatus(key) === "present"}
            class:bg-green-500={getLetterStatus(key) === "correct"}
          >
            {key}
          </div>
        {/each}
      </div>
    {/each}
  </div>
</div>
