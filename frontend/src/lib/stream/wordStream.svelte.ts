import { getGame } from "../game/game.svelte";
import { GameState, type IncomingWord } from "../game/game.types";

let incomingWords: IncomingWord[] = $state([]);
let myword = $state<string>("");

let mywordtimeout = $state<number | null>(null);

export function resetMyWordTimeout() {
	mywordtimeout = null;
}

export function setMyWord(word: string) {
	myword = word;
	if (mywordtimeout != null) {
		clearTimeout(mywordtimeout);
		mywordtimeout = null;
	}
	else {
		mywordtimeout = setTimeout(() => {
			const game = getGame();
			if (game.State == GameState.VotingInProgress)
				addWords([myword]);
		}, 2000);
	}
}

export function addWords(newWords: string[]) {
	const now = Date.now();
	const wordsToAdd: IncomingWord[] = newWords.map((word, index) => {
		let isMyword = false;
		if (myword == word) {
			isMyword = true;
			myword = "";
			if (mywordtimeout != null) {
				clearTimeout(mywordtimeout);
				mywordtimeout = null;
			}
		}
		return {
			id: `incoming-${word}-${now}-${index}`,
			word: word.toUpperCase(),
			timestamp: now + (index * 50),
			x: Math.random() * 80 + 10,
			myWord: isMyword
		}
	});

	incomingWords = [...incomingWords, ...wordsToAdd].slice(-12);
}

export function resetWords() {
	incomingWords = [];
}

export function getWords() {
	return incomingWords;
}

export function setWords(newWords: IncomingWord[]) {
	incomingWords = newWords;
}
