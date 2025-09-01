export type Game = {
	State: GameState;
	Board: Row[];
	SelectedWord: string | null;
	Round: number;
	TopWords: Vote[];
	TimeToNextGame: number;
}

export enum GameState {
	WaitingForVote,
	VotingInProgress,
	Won,
	Lost,
}

export type Row = {
	Letters: string[];
	state: BlockState[];
}

export enum BlockState {
	Absent,
	Present,
	Correct
}

export enum ConnectionState {
	Disconnected,
	Connecting,
	Connected,
	Unauthorized,

}

export type Vote = {
	Word: string;
	Count: number;
	Percentage: number;
}

export enum ServerMessageType {
	InitialState,
	VotingStarted,
	GameUpdate,
	LiveData,
	GameStarting,
	VoteStream,
	Response
}

export enum RequestResponseType {
	Success,
	VotingNotAllowed,
	InvalidVote,
	AlreadyVoted
}

export type LiveData = {
	userCount: number;
	totalVotes: number;
	myVote: string | null;
	topWords: Vote[];
};

export type InitialState = {
	game: Game;
	liveData: LiveData;
};

export type IncomingWord = {
	id: string;
	word: string;
	timestamp: number;
	x: number;
	myWord: boolean;
};

export enum SubmitState {
	Idle,
	Submitting,
	Success,
	Error,
	Submitted,
}