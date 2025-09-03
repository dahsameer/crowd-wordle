<script lang="ts">
  const { close } = $props();

  function closeModal() {
    close();
  }

  function handleKeydown(event: KeyboardEvent) {
    if (event.key === "Escape") {
      closeModal();
    }
  }

  function handleBackdropClick(event: MouseEvent) {
    if (event.target === event.currentTarget) {
      closeModal();
    }
  }
</script>

<svelte:window on:keydown={handleKeydown} />

<div
  class="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center p-4"
  onclick={handleBackdropClick}
  onkeydown={handleKeydown}
  role="dialog"
  tabindex="-1"
  aria-modal="true"
  aria-labelledby="info-title"
>
  <div
    class="bg-gray-900 rounded-2xl border border-white/20 shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-hidden"
  >
    <div class="p-6 border-b border-white/10 bg-blue-900/50">
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <h2 id="info-title" class="text-2xl font-bold text-white mb-3">
            About Crowd Wordle
          </h2>
          
          <!-- Links moved below title with better spacing -->
          <div class="flex flex-wrap items-center gap-4 text-sm">
            <div class="flex items-center gap-1.5 text-gray-400">
              <span>by</span>
              <a
                target="_blank"
                href="https://sameerdahal.com"
                class="text-blue-300 hover:text-blue-200 underline underline-offset-2 decoration-blue-300/50 hover:decoration-blue-200 transition-colors duration-200 flex items-center gap-1"
              >
                Sameer Dahal
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="w-3 h-3"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                  />
                </svg>
              </a>
            </div>

            <div class="text-gray-500">•</div>

            <a
              target="_blank"
              href="https://github.com/dahsameer/crowd-wordle"
              class="text-gray-300 hover:text-white transition-colors duration-200 flex items-center gap-1.5 px-2 py-1 rounded-md hover:bg-white/10"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="w-4 h-4"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
              </svg>
              View on GitHub
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="w-3 h-3 opacity-60"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                />
              </svg>
            </a>
          </div>
        </div>

        <button
          onclick={closeModal}
          class="p-2 hover:bg-white/10 rounded-lg transition-colors duration-200 ml-4"
          aria-label="Close info"
        >
          <svg
            class="w-6 h-6 text-gray-400 hover:text-white"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>
    </div>

    <div class="p-6 overflow-y-auto max-h-[calc(90vh-120px)] space-y-6">
      <section>
        <h3
          class="text-lg font-semibold text-white mb-3 flex items-center gap-2"
        >
          <span class="text-xl">•</span>
          Overview
        </h3>
        <p class="text-gray-300 leading-relaxed">
          Wordle but you can play with other folks. get inspired by other's vote
          or get mad because nobody else is using their brain
        </p>
      </section>

      <section>
        <h3
          class="text-lg font-semibold text-white mb-3 flex items-center gap-2"
        >
          <span class="text-xl">•</span>
          How to Play
        </h3>
        <div class="space-y-3">
          <div class="flex gap-3">
            <div>
              <p class="text-white font-medium">
                I hope this is game is intuitive enough to play without any
                instructions, but if it isn't... here goes the instruction:
              </p>
              <ul class="list-disc list-inside mt-2 space-y-2">
                <li class="text-gray-400 text-sm">
                  Type a valid 5-letter word in the input box below the game
                  board.
                </li>
                <li class="text-gray-400 text-sm">
                  Wait till the vote timer runs out.
                </li>
                <li class="text-gray-400 text-sm">
                  The highest voted word gets played automatically.
                </li>
                <li class="text-gray-400 text-sm">
                  Watch the results on the game board!
                </li>
                <li class="text-gray-400 text-sm">
                  Collaborate with other players to guess the secret word within
                  6 tries.
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      <section>
        <h3
          class="text-lg font-semibold text-white mb-3 flex items-center gap-2"
        >
          <span class="text-xl">•</span>
          Color Guide
        </h3>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div
            class="flex items-center gap-3 p-3 bg-green-500/20 rounded-lg border border-green-500/30"
          >
            <div
              class="w-8 h-8 bg-green-500 rounded flex items-center justify-center text-white font-bold"
            >
              A
            </div>
            <div>
              <p class="text-green-300 font-medium text-sm">Correct</p>
              <p class="text-green-400/80 text-xs">
                Right letter, right position
              </p>
            </div>
          </div>

          <div
            class="flex items-center gap-3 p-3 bg-yellow-500/20 rounded-lg border border-yellow-500/30"
          >
            <div
              class="w-8 h-8 bg-yellow-500 rounded flex items-center justify-center text-white font-bold"
            >
              B
            </div>
            <div>
              <p class="text-yellow-300 font-medium text-sm">Present</p>
              <p class="text-yellow-400/80 text-xs">
                Right letter, wrong position
              </p>
            </div>
          </div>

          <div
            class="flex items-center gap-3 p-3 bg-gray-600/20 rounded-lg border border-gray-600/30"
          >
            <div
              class="w-8 h-8 bg-gray-600 rounded flex items-center justify-center text-white font-bold"
            >
              C
            </div>
            <div>
              <p class="text-gray-300 font-medium text-sm">Absent</p>
              <p class="text-gray-400/80 text-xs">Letter not in word</p>
            </div>
          </div>
        </div>
      </section>

      <section>
        <h3
          class="text-lg font-semibold text-white mb-3 flex items-center gap-2"
        >
          <span class="text-xl">•</span>
          Pro Tips
        </h3>
        <div class="space-y-2">
          <div class="flex items-start gap-3 p-3 bg-blue-500/10 rounded-lg">
            <div
              class="w-2 h-2 bg-blue-400 rounded-full mt-2 flex-shrink-0"
            ></div>
            <p class="text-gray-300 text-sm">
              Watch the realtime stream for inspiration from other players
            </p>
          </div>
        </div>
      </section>
    </div>

    <div class="p-4 border-t border-white/10 bg-gray-900/50">
      <button
        onclick={closeModal}
        class="w-full py-3 bg-orange-500 hover:bg-orange-600 text-white font-bold rounded-lg transition-all duration-200 hover:scale-[1.02]"
      >
        Got it!
      </button>
    </div>
  </div>
</div>