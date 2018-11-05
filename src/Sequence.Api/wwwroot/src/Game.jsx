import PropTypes from 'prop-types';
import React from 'react';
import { ServerContext } from "./contexts";

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
      const result = await this.context.playCardAsync(gameId, selectedCard, coord);

      if (result.error) {
        alert(result.error);
      } else {
        this.setState({
          selectedCard: null
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
    this._sse = new EventSource(`/api/games/${gameId}/stream?playerId=${playerId}`);
    this._sse.addEventListener('game-updated', () => this.loadGameAsync());
  }

  closeSse() {
    if (this._sse) {
      this._sse.close();
      this._sse = null;
    }
  }

  async componentDidMount() {
    await this.initAsync();
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
    this.closeSse();
  }

  render() {
    return (
      <GameView
        game={this.state.game}
        onCardClick={this.handleCardClick}
        onCoordClick={this.handleCoordClick}
        playerId={this.context.userName}
        selectedCard={this.state.selectedCard}
      />
    );
  }
}

class GameView extends React.PureComponent {
  static propTypes = {
    game: PropTypes.object,
    onCardClick: PropTypes.func.isRequired,
    onCoordClick: PropTypes.func.isRequired,
    playerId: PropTypes.string.isRequired,
    selectedCard: PropTypes.object,
  };

  render() {
    const { game, onCardClick, onCoordClick, playerId, selectedCard } = this.props;

    if (game) {
      const opponentObj = game.players.find(p => p.id !== playerId);

      const playerObj = {
        hand: game.hand,
        id: playerId,
        team: game.team,
      };

      return (
        <div id="game">
          <OpponentView {...opponentObj} />
          <BoardView board={game.board} chips={game.chips} onCoordClick={onCoordClick} />
          <PlayerView onCardClick={onCardClick} selectedCard={selectedCard} {...playerObj} />
          <DeckView discards={game.discards} numberOfCardsInDeck={game.numberOfCardsInDeck} />
        </div>
      );
    } else {
      return (
        <div>Loading...</div>
      );
    }
  }
}

class OpponentView extends React.PureComponent {
  static propTypes = {
    id: PropTypes.string.isRequired,
    numberOfCards: PropTypes.number.isRequired,
    team: PropTypes.oneOf(['red', 'green']).isRequired,
  };

  render() {
    const { id, numberOfCards, team } = this.props;

    const Card = () => <div className="card card-back"></div>;
    const Hand = () => Array(numberOfCards).fill().map((_, idx) => <Card key={idx} />);

    return (
      <div id="opponent" className="player" data-team={team}>
        <span className="player-name">
          {id}
        </span>

        <div id="opponent-hand" className="hand">
          <Hand />
        </div>
      </div>
    );
  }
}

class BoardView extends React.PureComponent {
  static propTypes = {
    board: PropTypes.arrayOf(
      PropTypes.arrayOf(PropTypes.array).isRequired,
    ).isRequired,

    chips: PropTypes.arrayOf(PropTypes.shape({
      coord: PropTypes.shape({
        column: PropTypes.number.isRequired,
        row: PropTypes.number.isRequired,
      }),

      isLocked: PropTypes.bool.isRequired,
      team: PropTypes.oneOf(['red', 'green']).isRequired,
    })).isRequired,

    onCoordClick: PropTypes.func.isRequired,
  };

  render() {
    const { board, chips, onCoordClick } = this.props;

    const numRows = board.length;
    const numCols = Math.max.apply(Math, board.map(r => r.length));

    const style = {
      'gridTemplateColumns': `repeat(${numCols}, 67px)`,
      'gridTemplateRows': `repeat(${numRows}, 50px)`,
    };



    // TODO: Memoize cells.
    const cells = board.map((cells, row) => {
      return cells.map((cell, column) => {
        const chip = chips.find(chip => chip.coord.row === row && chip.coord.column === column);
        const tile = cell === null ? null : { ...cell };
        return { tile, row, column, chip };
      });
    }).flat().map(({ tile, row, column, chip }, idx) => {
      if (tile) {
        chip = chip || { team: null, isLocked: false };

        return (
          <div
            key={idx}
            className="cell"
            data-suit={tile[0]}
            data-rank={tile[1]}
            data-chip={chip.team}
            data-sequence={chip.isLocked}
            onClick={() => onCoordClick({ column, row })}>
          </div>
        );
      } else {
        return <div key={idx} className="cell" data-joker></div>;
      }
    });

    return (
      <div id="board" style={style}>
        {cells}
      </div>
    )
  }
}

class PlayerView extends React.PureComponent {
  static propTypes = {
    hand: PropTypes.arrayOf(PropTypes.shape({
      deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
      suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
      rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
    })).isRequired,
    id: PropTypes.string.isRequired,
    onCardClick: PropTypes.func.isRequired,
    selectedCard: PropTypes.shape({
      deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
      suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
      rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
    }),
    team: PropTypes.oneOf(['red', 'green']).isRequired,
  };

  render() {
    const { hand, id, onCardClick, selectedCard, team } = this.props;

    const Card = ({ card }) => {
      return (
        <div
          className="card"
          data-suit={card.suit}
          data-rank={card.rank}
          data-selected={card === selectedCard}
          onClick={() => onCardClick(card)}
        ></div>
      );
    };

    const Hand = () => hand.map((card, idx) => <Card key={idx} card={card} />);

    return (
      <div id="user" className="player" data-team={team}>
        <span className="player-name">{id}</span>

        <div id="user-hand" className="hand">
          <Hand />
        </div>
      </div>
    );
  }
}

class DeckView extends React.PureComponent {
  static propTypes = {
    discards: PropTypes.arrayOf(PropTypes.shape({
      deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
      suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
      rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
    })).isRequired,
    numberOfCardsInDeck: PropTypes.number.isRequired,
  };

  render() {
    const { numberOfCardsInDeck } = this.props;

    return (
      <div id="deck" className="card card-back" title={`${numberOfCardsInDeck} cards remain in the deck.`}>
        {numberOfCardsInDeck}
      </div>
    );
  }
}

export default Game;
