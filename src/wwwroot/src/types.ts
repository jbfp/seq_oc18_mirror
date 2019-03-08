export enum BoardType {
    OneEyedJack = 'oneEyedJack',
    Sequence = 'sequence',
}

export type Board = Array<Array<Tile>>;

export type GameId = string;

export interface Tile {
    suit: any;
    rank: any;
}

export interface Coord {
    column: number;
    row: number;
}

export enum DeckNo {
    One = 'one',
    Two = 'two',
}

export enum Suit {
    Hearts = 'hearts',
    Spades = 'spades',
    Diamonds = 'diamonds',
    Clubs = 'clubs',
}

export enum Rank {
    Ace = 'ace',
    Two = 'two',
    Three = 'three',
    Four = 'four',
    Five = 'five',
    Six = 'six',
    Seven = 'seven',
    Eight = 'eight',
    Nine = 'nine',
    Ten = 'ten',
    Jack = 'jack',
    Queen = 'queen',
    King = 'king',
}

export interface Card {
    deckNo: DeckNo;
    suit: Suit;
    rank: Rank;
}

export enum Team {
    Red = 'red',
    Green = 'green',
    Blue = 'blue',
}

export interface Chip {
    coord: Coord;
    isLocked: boolean;
    team: Team;
}

export interface Move {
    byPlayerId: PlayerId;
    cardUsed: Card;
    coord: Coord;
    index: number;
}

export type PlayerId = number;
export type PlayerHandle = string;

export enum PlayerType {
    User = 'user',
    Bot = 'bot',
}

export interface Player {
    id: PlayerId;
    handle: PlayerHandle;
    numberOfCards: number;
    team: Team;
    type: PlayerType;
}

export interface Rules {
    boardType: BoardType;
    winCondition: number;
}

export interface GameState {
    chips: Chip[];
    currentPlayerId: PlayerId | null;
    deadCards: Card[];
    discards: Card[];
    hand: Card[];
    hasExchangedDeadCard: boolean;
    index: number;
    moves: Move[];
    numberOfCardsInDeck: number;
    numberOfSequencesToWin: number;
    playerId: PlayerId;
    players: Player[];
    rules: Rules;
    team: Team;
    version: number;
    winner: Team | null;
}

export interface Sequence {
    coords: Coord[];
    team: Team;
}

export interface CardPlayed {
    byPlayerId: number;
    cardDrawn: Card | boolean;
    cardUsed: Card;
    chip: Chip;
    coord: Coord;
    index: number;
    nextPlayerId: PlayerId | null;
    sequences: Sequence[];
    winner: Team | null;
}

export enum LoadGameResponseKind {
    Ok, NotChanged, NotFound, Error
}

export interface OkLoadGameResponse {
    kind: LoadGameResponseKind.Ok;
    game: GameState;
    board: Board;
}

export interface NotChangedLoadGameResponse {
    kind: LoadGameResponseKind.NotChanged;
}

export interface NotFoundLoadGameResponse {
    kind: LoadGameResponseKind.NotFound;
}

export interface ErrorLoadGameResponse {
    kind: LoadGameResponseKind.Error;
    error: Error;
}

export type LoadGameResponse =
    | OkLoadGameResponse
    | NotChangedLoadGameResponse
    | NotFoundLoadGameResponse
    | ErrorLoadGameResponse;
