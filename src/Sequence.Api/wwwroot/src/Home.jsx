import PropTypes from 'prop-types';
import React from 'react';
import { Route } from 'react-router-dom';

import { ServerContext } from './contexts';

import Game from './Game';
import Games from './Games';

class Home extends React.Component {
  static contextType = ServerContext;

  static propTypes = {
    onLogout: PropTypes.func.isRequired,
  };

  state = {
    games: null,
  };

  handleLogout = event => {
    event.preventDefault();
    this.props.onLogout();
  };

  async componentDidMount() {
    this.setState({ games: await this.context.getGamesAsync() });
  }

  render() {
    return (
      <div id="home" style={{ 'display': 'flex' }}>
        <div id="sidebar">
          <form onSubmit={this.handleLogout}>
            <button type="submit">
              Log out
            </button>
          </form>

          <Games games={this.state.games} />
        </div>

        <div style={{ 'flex': 1 }}>
          <Route path="/games/:id" render={props => <Game {...props} />} />
        </div>
      </div>
    );
  }
}

export default Home;
