import PropTypes from 'prop-types';
import React from 'react';
import { NavLink } from 'react-router-dom';

class Games extends React.Component {
  static propTypes = {
    games: PropTypes.arrayOf(PropTypes.string),
  };

  render() {
    const { games } = this.props;

    if (games) {
      const $gameList = games.map(gameId => (
        <li key={gameId}>
          <NavLink to={`/games/${gameId}`} activeStyle={{ 'background': 'blue', 'color': 'white' }}>
            {gameId}
          </NavLink>
        </li>
      ));

      return (
        <div id="games">
          Games

          <ul>
            {$gameList}
          </ul>
        </div>
      );
    } else {
      return <div>Loading...</div>;
    }
  }
}

export default Games;
