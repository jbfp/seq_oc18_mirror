import PropTypes from 'prop-types';
import React from 'react';
import { ServerContext } from "../contexts";
import GameView from './GameView';

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
    this._sse = new EventSource(`${window.env.api}/games/${gameId}/stream?playerId=${playerId}`);
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

export default Game;
