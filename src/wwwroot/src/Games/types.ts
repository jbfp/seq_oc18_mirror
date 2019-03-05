import { GameId } from "../types";

export interface Game {
    currentPlayer: string;
    gameId: GameId;
    lastMoveAt: Date;
    opponents: string[];
}

export interface GameCollections {
    completedGames: Game[];
    theirTurn: Game[];
    yourTurn: Game[];
}
