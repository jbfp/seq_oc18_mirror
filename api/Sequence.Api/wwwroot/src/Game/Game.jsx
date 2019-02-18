import PropTypes from 'prop-types';
import React from 'react';
import * as SignalR from '@aspnet/signalr';
import { ServerContext } from "../contexts";
import GameView from './GameView';

// Keys that respond to a card in hand.
const numberKeys = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];

const DEFAULT_HIDE_TIMEOUT = 1000;
const CURRENT_PLAYER_HIDE_TIMEOUT = 5000;

async function startAsync(connection) {
  try {
    await connection.start();
  } catch (err) {
    setTimeout(async () => await startAsync(connection), 5000);
  }
}

class Game extends React.Component {
  static contextType = ServerContext;

  static propTypes = {
    match: PropTypes.shape({
      params: PropTypes.shape({
        id: PropTypes.string.isRequired,
      }).isRequired,
    }).isRequired,
  };

  state = {
    game: null,
    hideCards: false,
    selectedCard: null,
    showNotification: null,
  };

  _gameEventSource = null;
  _hideCardsTimeoutHandle = null;
  _touches = [];

  constructor(props) {
    super(props);

    const URL = `${window.env.api}/myHub`;

    const CONNECTION_OPTIONS = Object.freeze({
      skipNegotiation: true,
      transport: SignalR.HttpTransportType.WebSockets,
    });

    const connection = new SignalR.HubConnectionBuilder()
      .withUrl(URL, { ...CONNECTION_OPTIONS })
      .configureLogging(SignalR.LogLevel.Information)
      .build();

    connection.on('UpdateGame', this.handleGameUpdatedEvent);

    connection.onclose(async error => {
      if (error) {
        await startAsync(this);
      }
    });

    this._connection = connection;
  }

  handleCardClick = (card) => {
    if (this.state.selectedCard === card) {
      this.setState({ selectedCard: null });
    } else {
      this.setState({ selectedCard: card });
    }
  };

  handleCoordClick = async coord => {
    if (!coord) {
      throw new Error(`Coord '${coord}' is not valid.`);
    }

    const { selectedCard } = this.state;

    if (selectedCard) {
      const gameId = this.props.match.params.id;

      this._connection.off('UpdateGame', this.handleGameUpdatedEvent);

      try {
        const result = await this.context.playCardAsync(gameId, selectedCard, coord);

        if (result.error) {
          console.warn(result.error);
        } else {
          this.setState({ selectedCard: null });

          if (!this.apply(result)) {
            await this.loadGameAsync();
          }
        }
      } finally {
        this._connection.on('UpdateGame', this.handleGameUpdatedEvent);
      }
    }
  };

  handleKeyboardInput = event => {
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

  apply = event => {
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
    const discards = [...game.discards, cardUsed];

    let hand = [...game.hand];

    if (typeof cardDrawn === 'object') {
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
      if (cardDrawn !== null) {
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
      chips = chips.map(chip => {
        const containedInSequence = event.sequence.coords.some(coord =>
          coord.column === chip.coord.column &&
          coord.row === chip.coord.row);

        if (containedInSequence) {
          return { ...chip, isLocked: true };
        } else {
          return chip;
        }
      });
    }

    const moves = [{
      byPlayerId: event.byPlayerId,
      cardUsed: event.cardUsed,
      coord: event.coord,
    }, ...game.moves];

    const winner = event.winner;

    this.setState({
      game: {
        ...game,
        chips,
        currentPlayerId,
        discards,
        hand,
        moves,
        numberOfCardsInDeck,
        version,
        winner,
      },
    });

    return true;
  };

  handleGameUpdatedEvent = async gameEvent => {
    if (!this.apply(gameEvent)) {
      await this.loadGameAsync();
    }

    if (this.state.showNotification) {
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

  handleVisibilityChanged = async event => {
    if (!event.target.hidden) {
      await this.initAsync();
    }
  };

  async loadGameAsync() {
    const gameId = this.props.match.params.id;
    const game = await this.context.getGameByIdAsync(gameId);
    const board = await this.context.getBoardAsync(game.rules.boardType);
    this.setState({ game: { ...game, board } });
  }

  handleTouchStart = ({ changedTouches }) => {
    const numTouches = changedTouches.length;

    for (let i = 0; i < numTouches; i++) {
      const id = changedTouches[i].identifier;

      if (this._touches.indexOf(id) < 0) {
        this._touches.push(id);
      }
    }
  };

  handleTouchEnd = ({ changedTouches }) => {
    const numTouches = changedTouches.length;

    for (let i = 0; i < numTouches; i++) {
      const idx = this._touches.indexOf(changedTouches[i].identifier);

      if (idx < 0) {
        continue;
      }

      this._touches.splice(idx, 1);
    }
  };

  handleDeviceOrientation = ({ beta }) => {
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

  async initAsync() {
    await this.loadGameAsync();
    const gameId = this.props.match.params.id;
    const playerId = this.context.userName;
    await this.unsubscribeAsync();
    await this._connection.invoke('Subscribe', gameId);
  }

  async unsubscribeAsync() {
    if (this._connection.state === SignalR.HubConnectionState.Connected) {
      await this._connection.invoke('Unsubscribe', this.props.match.params.id);
    }
  }

  async componentDidMount() {
    if (typeof Notification !== 'undefined') {
      const handlePermissionCallback = result => {
        this.setState({ showNotification: result === 'granted' });
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

    await startAsync(this._connection);
    await this.initAsync();
  }

  async componentDidUpdate(prevProps) {
    const previousGameId = prevProps.match.params.id;
    const currentGameId = this.props.match.params.id;

    if (previousGameId !== currentGameId) {
      await this.initAsync();
    }
  }

  async componentWillUnmount() {
    document.removeEventListener('visibilitychange', this.handleVisibilityChanged, false);
    window.removeEventListener('keydown', this.handleKeyboardInput);
    window.removeEventListener('deviceorientation', this.handleDeviceOrientation, false);
    window.removeEventListener('touchstart', this.handleTouchStart, false);
    window.removeEventListener('touchend', this.handleTouchEnd, false);
    await this._connection.stop();
  }

  render() {
    return (
      <GameView
        game={this.state.game}
        hideCards={this.state.hideCards}
        onCardClick={this.handleCardClick}
        onCoordClick={this.handleCoordClick}
        selectedCard={this.state.selectedCard}
      />
    );
  }
}

export default Game;
