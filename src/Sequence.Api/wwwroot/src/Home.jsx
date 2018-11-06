import PropTypes from 'prop-types';
import React from 'react';
import { Route, withRouter } from 'react-router-dom';

import { ServerContext } from './contexts';

import Game from './Game';
import Games from './Games';

class Home extends React.Component {
  static contextType = ServerContext;

  static propTypes = {
    history: PropTypes.object.isRequired,
    onLogout: PropTypes.func.isRequired,
  };

  state = {
    games: null,
    opponent: '',
  };

  handleLogout = event => {
    event.preventDefault();
    this.props.onLogout();
  };

  handleOpponentChange = event => {
    this.setState({ opponent: event.target.value });
  };

  createGame = async event => {
    event.preventDefault();
    const opponent = this.state.opponent;
    this.setState({ opponent: '' });
    const gameId = await this.context.createGameAsync(opponent);
    await this.loadGamesAsync();
    this.props.history.push(`/games/${gameId}`);
  };

  async loadGamesAsync() {
    this.setState({ games: await this.context.getGamesAsync() });
  }

  async componentDidMount() {
    await this.loadGamesAsync();
  }

  render() {
    const { games, opponent } = this.state;

    return (
      <div id="home">
        <div id="sidebar">
          <form onSubmit={this.handleLogout}>
            <button type="submit">
              Log out
            </button>
          </form>

          <hr />

          <form onSubmit={this.createGame}>
            <fieldset>
              <legend>New game</legend>

              <div>
                <label>
                  Opponent:&nbsp;
                  <input
                    name="opponent"
                    type="text"
                    value={opponent}
                    onChange={this.handleOpponentChange}
                    autoComplete="false"
                  />
                </label>
              </div>

              <div>
                <button type="submit" disabled={!opponent}>
                  Create game
                </button>
              </div>
            </fieldset>
          </form>

          <hr />

          <Games games={games} />
        </div>

        <div id="main">
          <Route path="/games/:id" render={props => <Game {...props} />} />
        </div>
      </div>
    );
  }
}

export default withRouter(props => <Home {...props} />);
