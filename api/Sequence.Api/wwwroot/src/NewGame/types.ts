import { BoardType, GameId } from '../types';

export enum GameSize {
    OneVsOne = 1,
    OneVsOneVsOne = 2,
    TwoVsTwo = 3,
    TwoVsTwoVsTwo = 5,
}

export interface NewGameState {
    boardType: BoardType;
    numSequencesToWin: NumSequencesToWin | null;
    opponents: Opponent[];
    busy: boolean;
    error: string | null;
}

export enum BackgroundColor {
    None = '',
    Red = 'red',
    Green = 'green',
    Blue = 'blue',
}

export enum OpponentType { User = 0, Bot = 1 }

export interface Opponent {
    name: string;
    type: OpponentType;
}

export type NumSequencesToWin = 1 | 2 | 3 | 4;

export interface CreateGameForm {
    boardType: BoardType;
    numSequencesToWin: NumSequencesToWin;
    opponents: Opponent[];
}

export interface CanCreateGame {
    createGameAsync(form: CreateGameForm): Promise<GameId>;
}

export type BotType = string;

export interface CanGetBotTypes {
    getBotsAsync(): Promise<BotType[]>;
}
