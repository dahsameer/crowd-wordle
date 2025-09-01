import { showError } from "../shared/toast.svelte";
import { IsGameOver } from "../shared/utils";
import { addWords, resetMyWordTimeout, resetWords, setWords } from "../stream/wordStream.svelte";
import { getGame, getLiveData, setGame, setLiveData } from "./game.svelte";
import { BlockState, GameState, RequestResponseType, ServerMessageType, type InitialState, type LiveData, type Row } from "./game.types";
import { getMessageType, unpackGameUpdate, unpackInitial, unpackLiveData, unpackResponse, unpackVoteStream, unpackVotingStarted } from "./websocket/encoding";

export function handleData(buffer: Uint8Array) {

	const messageType = getMessageType(buffer);
	switch (messageType) {
		case ServerMessageType.InitialState:
			const data = unpackInitial(buffer);
			handleInitialMessage(data);
			break;
		case ServerMessageType.LiveData:
			const liveData = unpackLiveData(buffer);
			handleLiveDataMessage(liveData);
			break;
		case ServerMessageType.GameUpdate:
			const gameUpdate = unpackGameUpdate(buffer);
			handleGameUpdateMessage(gameUpdate);
			break;
		case ServerMessageType.GameStarting:
			handleNewGame();
			break;
		case ServerMessageType.Response:
			const response = unpackResponse(buffer);
			handleResponse(response);
			break;
		case ServerMessageType.VoteStream:
			const voteStream = unpackVoteStream(buffer);
			handleVoteStream(voteStream);
			break;
		case ServerMessageType.VotingStarted:
			const votingStarted = unpackVotingStarted(buffer);
			handleVotingStarted(votingStarted);
			break;
		default:
			console.warn("Unknown message type:", messageType);
	}
}

function handleInitialMessage(data: InitialState) {
	setGame(data.game);
	setLiveData(data.liveData);
}

function handleLiveDataMessage(data: LiveData) {
	setLiveData({
		...data,
		myVote: getLiveData().myVote,
	});
}

function handleVotingStarted(data: { round: number; timeRemaining: number }) {
	const gameState = getGame();
	setGame({
		...gameState,
		State: GameState.VotingInProgress,
		TimeToNextGame: data.timeRemaining
	});
}

function handleNewGame() {
	setGame({
		Board: [],
		Round: 0,
		SelectedWord: null,
		State: GameState.WaitingForVote,
		TimeToNextGame: 0,
		TopWords: []
	});
}

function handleResponse(response: RequestResponseType) {
	switch (response) {
		case RequestResponseType.Success:
			break;
		case RequestResponseType.AlreadyVoted:
			showError("Already Voted");
			break;
		case RequestResponseType.InvalidVote:
			showError("Invalid Vote");
			setLiveData({ ...getLiveData(), myVote: null });
			break;
		case RequestResponseType.VotingNotAllowed:
			showError("Voting Not allowed at the moment");
			setLiveData({ ...getLiveData(), myVote: null });
			break;
		default:
			console.warn("Unknown response type:", response);
	}
}

function handleGameUpdateMessage(gameUpdate: { state: GameState; round: number | null; word: string; wordStates: BlockState[]; selectedWord: string | null; timeRemaining: number }) {
	const gameState = getGame();
	if (!IsGameOver(gameUpdate.state)) {
		if (gameState.Round + 1 != gameUpdate.round) {
			throw new Error("Game update round mismatch");
		}
	}
	setLiveData({
		...getLiveData(),
		myVote: null
	});
	if (gameUpdate.round == null) {
		gameUpdate.round = gameState.Round + 1;
	}
	setGame({
		...gameState,
		State: gameUpdate.state,
		Board: gameState.Board.concat([{
			Letters: gameUpdate.word.split(""),
			state: gameUpdate.wordStates
		}]),
		SelectedWord: gameUpdate.selectedWord,
		Round: gameUpdate.round,
		TimeToNextGame: gameUpdate.timeRemaining
	});
	resetWords();
	resetMyWordTimeout();
}
function handleVoteStream(voteStream: string[]) {
	addWords(voteStream);
}

