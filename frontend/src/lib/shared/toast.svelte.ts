export type Toast = {
	show: boolean;
	message: string | null;
}

const toast = $state<Toast>({
	message: null,
	show: false
})

export function showError(message: string) {
	toast.message = message;
	toast.show = true;
}

export function hideToast() {
	toast.show = false;
}

export function getToast(): Toast {
	return toast;
}