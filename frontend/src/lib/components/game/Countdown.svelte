<script lang="ts">
  import { onMount, onDestroy } from "svelte";

  interface Props {
    targetDate: Date | string;
    positive: boolean | null;
    behindText: string | null;
    finishText?: string;
  }

  let { targetDate, positive, behindText, finishText }: Props = $props();

  let secondsRemaining: number = $state(0);
  let pulse: boolean = $state(false);
  let showFinishText: boolean = $state(false);
  let isPositive: boolean = $state(positive ?? true);
  let isExpired: boolean = $state(false);
  let interval: number | undefined;

  $effect(() => {
    const target: Date =
      targetDate instanceof Date ? targetDate : new Date(targetDate);
    updateCountdown(target);
  });

  function updateCountdown(target: Date): void {
    const now: number = new Date().getTime();
    const targetTime: number = target.getTime();
    const difference: number = Math.floor((targetTime - now) / 1000);

    if (difference <= 3) {
      pulse = true;
    }

    if (difference < 0) {
      secondsRemaining = 0;
      isExpired = true;
      showFinishText = finishText != null;
    } else {
      secondsRemaining = difference;
      isExpired = false;
    }
  }

  onMount(() => {
    interval = setInterval(() => {
      const target: Date =
        targetDate instanceof Date ? targetDate : new Date(targetDate);
      updateCountdown(target);
    }, 100);
  });

  onDestroy(() => {
    if (interval) {
      clearInterval(interval);
    }
  });
</script>

<span
  class="font-bold text-amber-50"
  class:animate-pulse={pulse}
  class:text-green-500={isPositive && (isExpired || pulse)}
  class:text-red-500={!isPositive && (isExpired || pulse)}
  >{showFinishText ? finishText : secondsRemaining}{showFinishText
    ? ""
    : behindText}</span
>
