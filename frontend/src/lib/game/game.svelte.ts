import { ConnectionState, GameState, type LiveData, type Game, type Vote } from "./game.types";

let game = $state<Game>({
	Round: 0,
	Board: [],
	SelectedWord: null,
	TopWords: [],
	State: GameState.WaitingForVote,
	TimeToNextGame: 0
});

let vote = $state<LiveData>({
	userCount: 1,
	totalVotes: 0,
	myVote: null,
	topWords: [],
});

let connection = $state<ConnectionState>(ConnectionState.Disconnected);

export function setConnectionState(newState: ConnectionState) {
	connection = newState;
}

export function getConnectionState(): ConnectionState {
	return connection;
}

export function getLiveData(): LiveData {
	return vote;
}

export function setLiveData(newState: LiveData) {
	vote = newState;
}

export let stream = $state<Vote[]>([]);

export function setGame(newState: Game) {
	game = newState;
}

export function getGame(): Game {
	return game;
}