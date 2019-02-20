import React from 'react';
import { RouteComponentProps } from "react-router";
import * as SignalR from '@aspnet/signalr';
import * as t from "../types";
import { ServerContext } from "../contexts";
import { GameEvent } from './types';
import GameView from './GameView';

// Keys that respond to a card in hand.
const numberKeys = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];

const DEFAULT_HIDE_TIMEOUT = 1000;
const CURRENT_PLAYER_HIDE_TIMEOUT = 5000;

const HUB_URL = `${window.env.api}/myHub`;

const CONNECTION_OPTIONS: SignalR.IHttpConnectionOptions = {
    skipNegotiation: true,
    transport: SignalR.HttpTransportType.WebSockets,
    logMessageContent: true,
};

async function startAsync(connection: SignalR.HubConnection, callback: () => Promise<void>) {
    try {
        await connection.start();
    } catch (err) {
        setTimeout(async () => await startAsync(connection, callback), 5000);
    }

    await callback();
}

interface GameProps {
    board: t.Board;
    game: t.GameState;
    id: t.GameId;
    onRequestReload: () => Promise<void>;
}

interface GameState {
    game: t.GameState;
    hideCards: boolean;
    selectedCard: t.Card | null;
}

export default class Game extends React.Component<GameProps, GameState> {
    static contextType = ServerContext;

    private readonly _connection: SignalR.HubConnection;
    private readonly _touches: number[] = [];

    private _hideCardsTimeoutHandle: number | null = null;
    private _showNotification = false;

    constructor(props: GameProps) {
        super(props);

        this.state = {
            game: { ...props.game },
            hideCards: false,
            selectedCard: null,
        };

        const connection = new SignalR.HubConnectionBuilder()
            .withUrl(HUB_URL, { ...CONNECTION_OPTIONS })
            .configureLogging(SignalR.LogLevel.Information)
            .build();

        connection.on('UpdateGame', () => { });
        connection.on('UpdateGame', this.handleGameUpdatedEvent);

        connection.onclose(async error => {
            if (error) {
                await startAsync(connection, () => this.handleConnectionStarted());
            }
        });

        this._connection = connection;
    }

    handleCardClick = (card: t.Card) => {
        if (this.state.selectedCard === card) {
            this.setState({ selectedCard: null });
        } else {
            this.setState({ selectedCard: card });
        }
    };

    handleCoordClick = async (coord: t.Coord) => {
        if (!coord) {
            throw new Error(`Coord '${coord}' is not valid.`);
        }

        const { selectedCard } = this.state;

        if (selectedCard) {
            this._connection.off('UpdateGame', this.handleGameUpdatedEvent);

            try {
                const result = await this.context.playCardAsync(this.props.id, selectedCard, coord);

                if (result.error) {
                    console.warn(result.error);
                } else {
                    this.setState({ selectedCard: null });

                    if (!this.apply(result)) {
                        await this.props.onRequestReload();
                    }
                }
            } finally {
                this._connection.on('UpdateGame', this.handleGameUpdatedEvent);
            }
        }
    };

    handleKeyboardInput = (event: KeyboardEvent) => {
        if (!this.state.game) {
            return;
        }

        const { key } = event;

        // Ignore event if CTRL, SHIFT, etc. is pressed as well.
        if (event.ctrlKey || event.shiftKey || event.altKey || event.metaKey) {
            return;
        }

        if (numberKeys.includes(key)) {
            const num = Number.parseInt(key, 10);
            const { hand } = this.state.game;

            if (num <= hand.length) {
                event.preventDefault();
                this.handleCardClick(hand[num - 1]);
            }
        }
    };

    apply = (event: GameEvent) => {
        const game = this.state.game;

        if (event.index === game.version) {
            return true;
        }

        if (event.index - game.version > 1) {
            return false;
        }

        const cardDrawn = event.cardDrawn;
        const cardUsed = event.cardUsed;
        const currentPlayerId = event.nextPlayerId;

        let hand = [...game.hand];

        function isCard(x: any): x is t.Card | null {
            return x === null || (typeof x === 'object' && typeof x !== 'boolean');
        }

        if (isCard(cardDrawn)) {
            // This user performed this action. Remove the used card and add the drawn card.
            const indexOfCardUsed = hand.findIndex(c =>
                c.deckNo === cardUsed.deckNo &&
                c.suit === cardUsed.suit &&
                c.rank === cardUsed.rank);

            // Remove used card.
            hand = [
                ...hand.slice(0, indexOfCardUsed),
                ...hand.slice(indexOfCardUsed + 1),
            ];

            // Add card drawn. cardDrawn can be null if there are no more cards in the deck.
            // TODO: Use discard pile.
            if (cardDrawn) {
                hand = [...hand, cardDrawn];
            }
        }

        const numberOfCardsInDeck = game.numberOfCardsInDeck - (event.cardDrawn ? 1 : 0);
        const version = event.index;

        // Update board.
        let chips = [...game.chips];

        if (event.chip === null) {
            const indexToRemove = chips.findIndex(({ coord }) =>
                coord.column === event.coord.column &&
                coord.row === event.coord.row);

            chips = [
                ...chips.slice(0, indexToRemove),
                ...chips.slice(indexToRemove + 1),
            ];
        } else if (typeof event.chip === 'string') {
            chips = [...chips, {
                coord: event.coord,
                isLocked: false,
                team: event.chip,
            }];
        }

        if (event.sequence) {
            const sequence = event.sequence;

            chips = chips.map(chip => {
                const containedInSequence = sequence.coords.some(coord =>
                    coord.column === chip.coord.column &&
                    coord.row === chip.coord.row);

                if (containedInSequence) {
                    return { ...chip, isLocked: true };
                } else {
                    return chip;
                }
            });
        }

        const moves: t.Move[] = [{
            byPlayerId: event.byPlayerId,
            cardUsed: event.cardUsed,
            coord: event.coord,
            index: event.index,
        }, ...game.moves];

        const winner = event.winner;

        const newGame: t.GameState = {
            ...game,
            chips,
            currentPlayerId,
            hand,
            moves,
            numberOfCardsInDeck,
            version,
            winner,
        };

        this.setState({
            game: newGame,
        });

        return true;
    };

    handleGameUpdatedEvent = async (event: GameEvent) => {
        if (!this.apply(event)) {
            await this.props.onRequestReload();
        }

        if (this._showNotification) {
            const { game } = this.state;
            const isCurrentPlayer = game.currentPlayerId === game.playerId;

            if (isCurrentPlayer) {
                new Notification('Sequence', {
                    body: 'It\'s your turn!',
                    icon: '/favicon.ico',
                    lang: 'en',
                    tag: 'your-turn',
                });
            }
        }
    };

    handleTouchStart = ({ changedTouches }: TouchEvent) => {
        const numTouches = changedTouches.length;

        for (let i = 0; i < numTouches; i++) {
            const id = changedTouches[i].identifier;

            if (this._touches.indexOf(id) < 0) {
                this._touches.push(id);
            }
        }
    };

    handleTouchEnd = ({ changedTouches }: TouchEvent) => {
        const numTouches = changedTouches.length;

        for (let i = 0; i < numTouches; i++) {
            const idx = this._touches.indexOf(changedTouches[i].identifier);

            if (idx < 0) {
                continue;
            }

            this._touches.splice(idx, 1);
        }
    };

    handleDeviceOrientation = ({ beta }: DeviceOrientationEvent) => {
        if (beta === null) {
            return;
        }

        const isFlat = Math.abs(beta) <= 18;
        const noTouching = this._touches.length === 0;

        if (isFlat && noTouching) {
            if (this._hideCardsTimeoutHandle) {
                return;
            }

            const { game } = this.state;
            const timeout = game && game.currentPlayerId === game.playerId
                ? CURRENT_PLAYER_HIDE_TIMEOUT
                : DEFAULT_HIDE_TIMEOUT;

            this._hideCardsTimeoutHandle = window.setTimeout(() => {
                this.setState({ hideCards: true });
            }, timeout);
        } else {
            if (this._hideCardsTimeoutHandle) {
                window.clearTimeout(this._hideCardsTimeoutHandle);
                this._hideCardsTimeoutHandle = null;
            }

            this.setState({ hideCards: false });
        }
    };

    async handleConnectionStarted() {
        await this._connection.invoke('Subscribe', this.props.id);
    }

    async componentDidMount() {
        if (typeof Notification !== 'undefined') {
            const handlePermissionCallback = (result: NotificationPermission) => {
                this._showNotification = result === 'granted'
            };

            const promise = Notification.requestPermission();

            if (typeof promise === 'undefined') {
                // Safari on MacOS only supports the old requestPermission function.
                Notification.requestPermission(handlePermissionCallback);
            } else {
                promise.then(handlePermissionCallback);
            }
        }

        window.addEventListener('deviceorientation', this.handleDeviceOrientation, false);
        window.addEventListener('touchstart', this.handleTouchStart, false);
        window.addEventListener('touchend', this.handleTouchEnd, false);
        window.addEventListener('keydown', this.handleKeyboardInput);

        await startAsync(this._connection, () => this.handleConnectionStarted());
    }

    async componentWillUnmount() {
        window.removeEventListener('keydown', this.handleKeyboardInput);
        window.removeEventListener('deviceorientation', this.handleDeviceOrientation, false);
        window.removeEventListener('touchstart', this.handleTouchStart, false);
        window.removeEventListener('touchend', this.handleTouchEnd, false);

        await this._connection.invoke('Unsubscribe', this.props.id);
        await this._connection.stop();
    }

    render() {
        return (
            <GameView
                board={this.props.board}
                game={this.state.game}
                hideCards={this.state.hideCards}
                onCardClick={this.handleCardClick}
                onCoordClick={this.handleCoordClick}
                selectedCard={this.state.selectedCard}
                userName={this.context.userName}
            />
        );
    }
}
