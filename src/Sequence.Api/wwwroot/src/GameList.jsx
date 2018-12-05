import React from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from "./contexts";
import './GameList.css';

class GameList extends React.Component {
    static contextType = ServerContext;

    state = {
        games: null,
    };

    async componentDidMount() {
        this.setState({
            games: await this.context.getGamesAsync(),
        });
    }

    render() {
        const { games } = this.state;

        if (games) {
            const GameListImpl = () => (
                <ul className="game-list">
                    {games.map((game, i) => (
                        <li className="game-list-item" key={i}>
                            <Link to={`/games/${game}`}>
                                {game}
                            </Link>
                        </li>
                    ))}
                </ul>
            );

            const EmptyGameList = () => (
                <span className="game-list-empty">
                    No games found
                </span>
            );

            const $gameList = games.length > 0
                ? <GameListImpl />
                : <EmptyGameList />;

            return (
                <div>
                    <p>
                        Pick a game to play from the list, or <Link to="/new-game">click here</Link> to start a new one.
                    </p>

                    {$gameList}
                </div>
            );
        } else {
            return (
                <div>
                    <p>Loading games...</p>
                </div>
            );
        }
    }
}

export default GameList;
