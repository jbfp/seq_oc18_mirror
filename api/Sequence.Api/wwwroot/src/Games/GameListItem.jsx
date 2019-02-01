import React from 'react';
import { Link } from 'react-router-dom';

const time = dt => <time dateTime={dt}>{dt.toLocaleString()}</time>;

class GameListItem extends React.PureComponent {
    render() {
        const { currentPlayer, gameId, lastMoveAt, opponents, userName } = this.props;
        const $opponents = opponents.join(', ');

        let $linkText;

        if (currentPlayer) {
            const $currentPlayer = currentPlayer === userName
                ? 'you!'
                : currentPlayer;

            const $time = lastMoveAt.valueOf() === 0 ? null : <span>(last move at {time(lastMoveAt)})</span>;

            $linkText = (
                <span>
                    You vs {$opponents}; current player is <strong>{$currentPlayer}</strong> {$time}
                </span>
            );
        } else {
            $linkText = (
                <span>
                    You vs {$opponents} (ended {time(lastMoveAt)})
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
