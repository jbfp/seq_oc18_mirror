import * as t from '../types';
import * as h from './helpers';

export interface GameState {
    board: t.Board;
    boardType: t.BoardType;
    chips: Map<string, t.Chip>;
    currentPlayerId: t.PlayerId | null;
    deadCards: Map<string, t.Card>;
    discards: t.Card[];
    firstPlayer: t.PlayerHandle;
    hand: Map<string, t.Card> | null;
    hasExchangedDeadCard: boolean;
    latestCardPlayed: Map<t.PlayerId, t.Card>;
    latestMoveAt: t.Coord | null;
    numCardsInDeck: number;
    playerHandle: t.PlayerHandle | null;
    playerId: t.PlayerId | null;
    players: t.Player[];
    playerTeam: t.Team | null;
    version: number;
    winCondition: number;
    winnerTeam: t.Team | null;
}

export function init(
    initialState: t.GameStarted,
    board: t.Board,
): Readonly<GameState> {
    const firstPlayer = initialState.players
        .filter((player) => player.id === initialState.firstPlayerId)
        .map((player) => player.handle)[0];

    const isInGame = initialState.playerId !== null;

    return Object.freeze({
        board,
        boardType: initialState.boardType,
        chips: new Map<string, t.Chip>(),
        currentPlayerId: initialState.firstPlayerId,
        deadCards: new Map<string, t.Card>(),
        discards: [],
        firstPlayer,
        hand: isInGame
            ? new Map<string, t.Card>(h.keyedArray({ xs: initialState.hand!, mapKey: h.cardKey }))
            : null,
        hasExchangedDeadCard: false,
        latestCardPlayed: new Map<t.PlayerId, t.Card>(),
        latestMoveAt: null,
        numCardsInDeck: initialState.numCardsInDeck,
        playerHandle: initialState.playerHandle,
        playerId: initialState.playerId,
        playerTeam: initialState.team,
        players: initialState.players,
        version: 0,
        winCondition: initialState.winCondition,
        winnerTeam: null,
    });
}

export function reducer(
    state: Readonly<GameState>,
    update: Readonly<t.GameUpdated>,
): Readonly<GameState> {
    return Object.freeze({
        ...update.gameEvents.reduce(reducerInternal, state),
        version: update.version,
    });
}

function reducerInternal(
    state: Readonly<GameState>,
    event: Readonly<t.GameEvents>,
): Readonly<GameState> {
    switch (event.kind) {
        case t.GameEventKind.CardDiscarded: {
            const card = event.card;
            const discards = [...state.discards, card];
            const latestCardPlayed = new Map<t.PlayerId, t.Card>(state.latestCardPlayed);
            latestCardPlayed.set(event.byPlayerId, event.card);
            const newState = { ...state, discards, latestCardPlayed };

            if (event.byPlayerId === state.playerId) {
                // Player discarded the card.
                const hand = new Map<string, t.Card>(state.hand!);
                hand.delete(h.cardKey(card));
                return { ...newState, hand };
            } else {
                // Opponent discarded the card.
                // TODO: Update opponent card count.
                return newState;
            }
        }

        case t.GameEventKind.ChipAdded: {
            const { coord, team } = event;
            const latestMoveAt = event.coord;
            const chips = new Map<string, t.Chip>(state.chips);
            const chip = { coord, team, isLocked: false };
            chips.set(h.coordKey(event.coord), chip);
            return { ...state, chips, latestMoveAt };
        }

        case t.GameEventKind.ChipRemoved: {
            const latestMoveAt = event.coord;
            const chips = new Map<string, t.Chip>(state.chips);
            chips.delete(h.coordKey(event.coord));
            return { ...state, chips, latestMoveAt };
        }

        case t.GameEventKind.CardDrawn: {
            const numCardsInDeck = state.numCardsInDeck - 1;
            const newState = { ...state, numCardsInDeck };

            if (event.card) {
                // Player drew the card.
                const hand = new Map<string, t.Card>(newState.hand!);
                const card = event.card;
                hand.set(h.cardKey(card), card);
                return { ...newState, hand };
            } else {
                // Opponent drew the card.
                // TODO: Update opponent card count.
                return newState;
            }
        }

        case t.GameEventKind.DeckShuffled: {
            return { ...state, numCardsInDeck: event.numCardsInDeck };
        }

        case t.GameEventKind.CardDied: {
            const card = event.card;
            const deadCards = new Map<string, t.Card>(state.deadCards);
            deadCards.set(h.cardKey(card), card);
            return { ...state, deadCards };
        }

        case t.GameEventKind.CardRevived: {
            const card = event.card;
            const deadCards = new Map<string, t.Card>(state.deadCards);
            deadCards.delete(h.cardKey(card));
            return { ...state, deadCards };
        }

        case t.GameEventKind.SequenceCreated: {
            const chips = new Map<string, t.Chip>(state.chips);
            const { coords, team } = event.sequence;

            for (const coord of coords) {
                const key = h.coordKey(coord);
                const chip = { coord, team, isLocked: true };
                chips.set(key, chip);
            }

            return { ...state, chips };
        }

        case t.GameEventKind.TurnEnded: {
            return { ...state, currentPlayerId: event.nextPlayerId };
        }

        case t.GameEventKind.GameEnded: {
            return { ...state, winnerTeam: event.winnerTeam };
        }

        default: {
            return state;
        }
    }
}
