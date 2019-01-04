import React from 'react';
import { Link } from 'react-router-dom';

class GameListItem extends React.PureComponent {
    render() {
        const { currentPlayer, gameId, opponents, userName } = this.props;
        const $opponents = opponents.join(', ');

        let $linkText;

        if (currentPlayer) {
            const $currentPlayer = currentPlayer === userName
                ? 'you!'
                : currentPlayer;

            $linkText = (
                <span>
                    You vs {$opponents}; current player is <strong>{$currentPlayer}</strong>
                </span>
            );
        } else {
            $linkText = (
                <span>
                    You vs {$opponents}
                </span>
            );
        }

        return (
            <div className="game-list-item">
                <Link to={`/games/${gameId}`}>{$linkText}</Link>
            </div>
        );
    }
}

export default GameListItem;
