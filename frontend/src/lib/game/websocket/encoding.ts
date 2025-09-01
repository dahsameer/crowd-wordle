import { BlockState, ConnectionState, GameState, RequestResponseType, ServerMessageType, type Game, type InitialState, type LiveData, type Row, type Vote } from "../game.types";
import { IsGameOver } from "../../shared/utils";

const WORD_LENGTH = 5;
const BASE_CHAR = 'a'.charCodeAt(0);
const BITS_PER_CHAR = 5;
const BITMASK = (1 << BITS_PER_CHAR) - 1; // 0x1F
const WORD_BITS = WORD_LENGTH * BITS_PER_CHAR;

export function encodeToPackedNum(word: string) {
	if (word.length !== WORD_LENGTH) return 0;
	word = word.toLowerCase();

	let packed = 0;
	for (let i = 0; i < WORD_LENGTH; i++) {
		const value = word.charCodeAt(i) - BASE_CHAR;
		if (value < 0 || value > 25) return 0;
		packed |= (value & BITMASK) << (i * BITS_PER_CHAR);
	}
	return packed >>> 0;
}

export function decodeFromPackedNum(packed: number): string {
	const letters: string[] = [];
	for (let i = 0; i < WORD_LENGTH; i++) {
		const value = (packed >> (i * BITS_PER_CHAR)) & BITMASK;
		letters.push(String.fromCharCode(BASE_CHAR + value));
	}
	return letters.join("").toUpperCase();
}
export function decodeFromArr(nums: number[]): string {
	let chars = '';
	for (let i = 0; i < 5; i++) {
		if (nums[i] > 25) return '';
		chars += String.fromCharCode(97 + nums[i]);
	}
	return chars;
}

export class BitReader {
	private offset = 0;
	private bitBuffer = 0n;
	private bitCount = 0;

	constructor(private buffer: Uint8Array) { }

	readBits(bits: number): number {
		if (bits <= 0) throw new Error("bits must be > 0");
		if (bits > 53) throw new Error("Cannot read more than 53 bits at once (JS number limit)");

		while (this.bitCount < bits) {
			if (this.offset >= this.buffer.length) {
				throw new Error("Unexpected end of buffer");
			}
			this.bitBuffer |= BigInt(this.buffer[this.offset++]) << BigInt(this.bitCount);
			this.bitCount += 8;
		}

		const mask = (1n << BigInt(bits)) - 1n;
		const value = this.bitBuffer & mask;
		this.bitBuffer >>= BigInt(bits);
		this.bitCount -= bits;

		return Number(value);
	}

	get hasMore(): boolean {
		return this.offset < this.buffer.length || this.bitCount > 0;
	}
}

export function getMessageType(buffer: Uint8Array): ServerMessageType {
	const reader = new BitReader(buffer);
	return reader.readBits(3) as ServerMessageType;
}

export function unpackInitial(buffer: Uint8Array): InitialState {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3);
	const gameState: GameState = reader.readBits(2) as GameState;
	const isGameOver = IsGameOver(gameState);
	const userCount = reader.readBits(16);
	let nextEventTime = 0;
	if (isGameOver || gameState == GameState.VotingInProgress) {
		nextEventTime = reader.readBits(4);
	}
	const round = reader.readBits(3);
	let selectedWord: string | null = null;
	if (isGameOver) {
		const packed = reader.readBits(WORD_BITS);
		selectedWord = decodeFromPackedNum(packed);
	}

	const board: Row[] = [];
	for (let r = 0; r < round; r++) {
		const row: Row = {
			Letters: [],
			state: []
		};
		const word = decodeFromPackedNum(reader.readBits(WORD_BITS));
		row.Letters = word.split('');
		for (let c = 0; c < WORD_LENGTH; c++) {
			const state = reader.readBits(2);
			row.state.push(state);
		}
		board.push(row);
	}

	let totalVotes = 0;
	let topWords: Vote[] = [];
	if (gameState == GameState.VotingInProgress || isGameOver) {
		totalVotes = reader.readBits(16);
		const topWordCount = reader.readBits(2);
		for (let i = 0; i < topWordCount; i++) {
			const packed = reader.readBits(WORD_BITS);
			const voteCount = reader.readBits(16);
			const word = decodeFromPackedNum(packed);
			topWords.push({
				Word: word,
				Count: voteCount,
				Percentage: totalVotes > 0 ? Math.round((voteCount / totalVotes) * 100) : 0
			});
		}
	}
	const didIVote = reader.readBits(1) === 1;
	let word: string | null = null;
	if (didIVote) {
		const packed = reader.readBits(WORD_BITS);
		word = decodeFromPackedNum(packed);
	}

	const gameData: Game = {
		Board: board,
		SelectedWord: selectedWord,
		Round: round,
		State: gameState,
		TimeToNextGame: nextEventTime,
		TopWords: topWords
	};
	const liveData: LiveData = {
		userCount,
		totalVotes,
		topWords,
		myVote: word,
	};

	return {
		game: gameData,
		liveData: liveData
	};
}

export function unpackResponse(buffer: Uint8Array): RequestResponseType {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3); // Skip message type
	const voteResult: RequestResponseType = reader.readBits(2) as RequestResponseType;

	return voteResult;
}

export function unpackVotingStarted(buffer: Uint8Array): { round: number; timeRemaining: number } {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3); // Skip message type
	const round = reader.readBits(3);
	const timeRemaining = reader.readBits(4);

	return {
		round,
		timeRemaining
	};
}

export function unpackGameUpdate(buffer: Uint8Array) {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3); // Skip message type
	const gameState: GameState = reader.readBits(2) as GameState;
	const isGameOver = IsGameOver(gameState);

	let round = null;
	if (!isGameOver) {
		round = reader.readBits(3);
	}
	let timeRemaining = 0;
	if (isGameOver) {
		timeRemaining = reader.readBits(4);
	}

	const wordPacked = reader.readBits(WORD_BITS);
	const word = decodeFromPackedNum(wordPacked);

	const wordStates: BlockState[] = [];
	for (let j = 0; j < WORD_LENGTH; j++) {
		const state = reader.readBits(2) as BlockState;
		wordStates.push(state);
	}

	let selectedWord: string | null = null;
	if (gameState == GameState.Lost) {
		const selectedWordPacked = reader.readBits(WORD_BITS);
		selectedWord = decodeFromPackedNum(selectedWordPacked);
	}

	return {
		state: gameState,
		round,
		word,
		wordStates,
		selectedWord,
		timeRemaining
	};
}

export function unpackLiveData(buffer: Uint8Array): LiveData {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3); // Skip message type
	const gameState: GameState = reader.readBits(2) as GameState;
	const isGameOver = IsGameOver(gameState);
	const userCount = reader.readBits(16);

	let totalVotes = 0;
	let topWords: Vote[] = [];

	if (gameState === GameState.VotingInProgress || isGameOver) {
		totalVotes = reader.readBits(16);
		const topWordCount = reader.readBits(2);

		for (let i = 0; i < topWordCount; i++) {
			const packed = reader.readBits(WORD_BITS);
			const voteCount = reader.readBits(16);
			const word = decodeFromPackedNum(packed);
			topWords.push({
				Word: word,
				Count: voteCount,
				Percentage: totalVotes > 0 ? Math.round((voteCount / totalVotes) * 100) : 0
			});
		}
	}

	return {
		userCount,
		totalVotes,
		topWords,
		myVote: null,
	};
}

export function unpackNewGame(buffer: Uint8Array): boolean {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3); // Skip message type

	return true;
}

export function unpackVoteStream(buffer: Uint8Array): string[] {
	const reader = new BitReader(buffer);

	const _ = reader.readBits(3); // Skip message type
	const wordCount = reader.readBits(4);
	const words: string[] = []
	for (let i = 0; i < wordCount; i++) {
		const wordInt = reader.readBits(WORD_BITS);
		const word = decodeFromPackedNum(wordInt);
		words.push(word);
	}
	return words;
}