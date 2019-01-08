import PropTypes from 'prop-types';
import React from 'react';
import { ServerContext } from "../contexts";
import GameView from './GameView';

// Keys that respond to a card in hand.
const numberKeys = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];

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
    selectedCard: null,
    showNotification: null,
  };

  _sse = null;

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

      this._sse.removeEventListener('game-updated', this.handleGameUpdatedEvent);

      try {
        const result = await this.context.playCardAsync(gameId, selectedCard, coord);

        if (result.error) {
          alert(result.error);
        } else {
          this.setState({ selectedCard: null });

          if (!this.apply(result)) {
            await this.loadGameAsync();
          }
        }
      } finally {
        this._sse.addEventListener('game-updated', this.handleGameUpdatedEvent);
      }
    }
  };

  handleKeyboardInput = event => {
    const { key } = event;

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

    const cardUsed = event.cardUsed;
    const currentPlayerId = event.nextPlayerId;
    const discards = [...game.discards, cardUsed];

    let hand = [...game.hand];

    if (typeof event.cardDrawn === 'object') {
      // This user performed this action. Remove the used card and add the drawn card.
      const indexOfCardUsed = hand.findIndex(c =>
        c.deckNo === cardUsed.deckNo &&
        c.suit === cardUsed.suit &&
        c.rank === cardUsed.rank);

      hand = [
        ...hand.slice(0, indexOfCardUsed),
        ...hand.slice(indexOfCardUsed + 1),
        event.cardDrawn,
      ];
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

    let winner = game.winner;

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

      winner = { team: event.sequence.team };
    }

    const latestMoveAt = event.coord;

    this.setState({
      game: {
        ...game,
        chips,
        currentPlayerId,
        discards,
        hand,
        latestMoveAt,
        numberOfCardsInDeck,
        version,
        winner,
      },
    });

    return true;
  };

  handleGameUpdatedEvent = async event => {
    if (!this.apply(JSON.parse(event.data))) {
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

  async loadGameAsync() {
    const gameId = this.props.match.params.id;
    const game = await this.context.getGameByIdAsync(gameId);
    this.setState({ game });
  }

  async initAsync() {
    await this.loadGameAsync();
    const gameId = this.props.match.params.id;
    const playerId = this.context.userName;
    this._sse = new EventSource(`${window.env.api}/games/${gameId}/stream?player=${playerId}`);
    this._sse.addEventListener('game-updated', this.handleGameUpdatedEvent);
  }

  closeSse() {
    if (this._sse) {
      this._sse.close();
      this._sse = null;
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
        Notification.requestPermission(callback);
      } else {
        promise.then(handlePermissionCallback);
      }
    }

    await this.initAsync();
    window.addEventListener('keypress', this.handleKeyboardInput);
  }

  async componentDidUpdate(prevProps) {
    const previousGameId = prevProps.match.params.id;
    const currentGameId = this.props.match.params.id;

    if (previousGameId !== currentGameId) {
      this.closeSse();
      await this.initAsync();
    }
  }

  componentWillUnmount() {
    window.removeEventListener('keypress', this.handleKeyboardInput);
    this.closeSse();
  }

  render() {
    return (
      <GameView
        game={this.state.game}
        onCardClick={this.handleCardClick}
        onCoordClick={this.handleCoordClick}
        selectedCard={this.state.selectedCard}
      />
    );
  }
}

export default Game;
