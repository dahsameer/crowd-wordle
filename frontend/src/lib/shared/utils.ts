import { GameState } from "../game/game.types";

export function IsGameOver(gameState: GameState): boolean {
	return gameState === GameState.Won || gameState === GameState.Lost;
}