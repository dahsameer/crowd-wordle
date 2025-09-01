<script lang="ts">
  import { getToast, hideToast } from "../../shared/toast.svelte";

  let { duration = 4000 } = $props();
  let timeoutId: number;

  let toast = $derived(getToast());

  $effect(() => {
    if (toast.show) {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(() => {
        hideToast();
      }, duration);
    }

    return () => clearTimeout(timeoutId);
  });
</script>

{#if toast.show}
  <div class="toast" role="alert">
    <span>{toast.message}</span>
    <button onclick={hideToast} aria-label="Close">&times;</button>
  </div>
{/if}

<style>
  .toast {
    position: fixed;
    top: 20px;
    left: 50%;
    transform: translateX(-50%);
    background: #dc2626;
    color: white;
    padding: 12px 16px;
    border-radius: 6px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    display: flex;
    align-items: center;
    gap: 12px;
    font-size: 14px;
    z-index: 1000;
    max-width: 400px;
    animation: slideDown 0.3s ease-out;
  }

  button {
    background: none;
    border: none;
    color: white;
    font-size: 20px;
    cursor: pointer;
    padding: 0;
    line-height: 1;
    opacity: 0.8;
  }

  button:hover {
    opacity: 1;
  }

  @keyframes slideDown {
    from {
      transform: translateX(-50%) translateY(-100%);
      opacity: 0;
    }
    to {
      transform: translateX(-50%) translateY(0);
      opacity: 1;
    }
  }
</style>
