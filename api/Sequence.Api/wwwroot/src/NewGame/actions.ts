import { BoardType } from "../types";
import { GameSize, OpponentType, NumSequencesToWin } from "./types";
import NewGame from "./NewGame";

export enum NewGameActionKind {
    SetBoardType = 'set-board-type',
    SetBusy = 'set-busy',
    SetError = 'set-error',
    SetGameSize = 'set-game-size',
    SetOpponentName = 'set-opponent-name',
    SetOpponentType = 'set-opponent-type',
    SetWinCondition = 'set-win-condition',
}

export interface SetBoardTypeAction {
    kind: NewGameActionKind.SetBoardType;
    newBoardType: BoardType;
}

export interface SetBusyAction {
    kind: NewGameActionKind.SetBusy;
    newBusy: boolean;
}

export interface SetErrorAction {
    kind: NewGameActionKind.SetError;
    newError: string | null;
}

export interface SetGameSizeAction {
    kind: NewGameActionKind.SetGameSize;
    newGameSize: GameSize;
}

export interface SetOpponentNameAction {
    kind: NewGameActionKind.SetOpponentName;
    index: number;
    newOpponentName: string;
}

export interface SetOpponentTypeAction {
    kind: NewGameActionKind.SetOpponentType;
    index: number;
    newOpponentType: OpponentType;
}

export interface SetWinConditionAction {
    kind: NewGameActionKind.SetWinCondition;
    newWinCondition: NumSequencesToWin;
}

export type NewGameAction =
    | SetBoardTypeAction
    | SetBusyAction
    | SetErrorAction
    | SetGameSizeAction
    | SetOpponentNameAction
    | SetOpponentTypeAction
    | SetWinConditionAction;

export function setBoardType(boardType: BoardType): SetBoardTypeAction {
    return { kind: NewGameActionKind.SetBoardType, newBoardType: boardType };
}

export function setBusy(busy: boolean): SetBusyAction {
    return { kind: NewGameActionKind.SetBusy, newBusy: busy };
}

export function setError(error: string | null): SetErrorAction {
    return { kind: NewGameActionKind.SetError, newError: error };
}

export function setGameSize(gameSize: GameSize): SetGameSizeAction {
    return { kind: NewGameActionKind.SetGameSize, newGameSize: gameSize };
}

export function setOpponentName(index: number, name: string): SetOpponentNameAction {
    return { kind: NewGameActionKind.SetOpponentName, index, newOpponentName: name };
}

export function setOpponentType(index: number, type: OpponentType): SetOpponentTypeAction {
    return { kind: NewGameActionKind.SetOpponentType, index, newOpponentType: type };
}

export function setWinCondition(winCondition: NumSequencesToWin): SetWinConditionAction {
    return { kind: NewGameActionKind.SetWinCondition, newWinCondition: winCondition };
}
