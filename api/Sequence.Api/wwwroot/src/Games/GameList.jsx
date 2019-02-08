import React from 'react';
import GameListItem from './GameListItem';

const $emptyGameList = (
    <span className="game-list-empty">
        No games found
    </span>
);

class GameList extends React.PureComponent {
    render() {
        const { games, userName } = this.props;

        if (games.length === 0) {
            return $emptyGameList;
        }

        return (
            <ul className="game-list">
                {games.map(game => (
                    <li key={game.gameId}>
                        <GameListItem {...game} userName={userName} />
                    </li>)
                )}
            </ul>
        );
    }
}

export default GameList;
