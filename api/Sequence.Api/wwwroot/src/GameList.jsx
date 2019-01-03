import React from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from "./contexts";
import './GameList.css';

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

class GameList extends React.Component {
    static contextType = ServerContext;

    state = {
        games: null,
        intervalId: null,
    };

    async loadGamesAsync() {
        const allGames = await this.context.getGamesAsync();
        const completedGames = [];
        const unfinishedGames = [];

        allGames.forEach(game => {
            if (game.currentPlayer) {
                unfinishedGames.push(game);
            } else {
                completedGames.push(game);
            }
        });

        this.setState({
            games: {
                unfinishedGames,
                completedGames,
            }
        });
    }

    async componentDidMount() {
        await this.loadGamesAsync();
        const intervalId = window.setInterval(() => this.loadGamesAsync(), 10000);
        this.setState({ intervalId });
    }

    componentWillUnmount() {
        const { intervalId } = this.state;

        if (intervalId) {
            window.clearInterval(intervalId);
        }
    }

    render() {
        const { games } = this.state;

        if (games) {
            const GameListImpl = ({ games, ...props }) => (
                <ol className="game-list" {...props}>
                    {games.map(game => (
                        <li key={game.gameId}>
                            <GameListItem {...game} userName={this.context.userName} />
                        </li>)
                    )}
                </ol>
            );

            const EmptyGameList = () => (
                <span className="game-list-empty">
                    No games found
                </span>
            );

            const $unfinishedGameList = games.unfinishedGames.length > 0
                ? <GameListImpl games={games.unfinishedGames} />
                : <EmptyGameList />;

            const $completedGameList = games.completedGames.length > 0
                ? <GameListImpl games={games.completedGames} />
                : <EmptyGameList />;

            return (
                <div>
                    <div>
                        <h3>Let's play</h3>

                        <p>
                            Pick a game to play from the list, or <Link to="/new-game">click here</Link> to start a new one.
                        </p>

                        <div>
                            {$unfinishedGameList}
                        </div>
                    </div>

                    <div>
                        <h3>Completed games</h3>

                        <div>
                            {$completedGameList}
                        </div>
                    </div>
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
