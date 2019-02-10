import React from 'react';
import { ServerContext } from "../contexts";
import GamesView from './GamesView';

class Games extends React.Component {
    static contextType = ServerContext;

    state = {
        games: null,
        intervalId: null,
    };

    async loadGamesAsync() {
        const userName = this.context.userName;
        const allGames = await this.context.getGamesAsync();
        const completedGames = [];
        const yourTurn = [];
        const theirTurn = [];

        allGames.map(game => {
            return {
                ...game,
                lastMoveAt: new Date(game.lastMoveAt)
            };
        }).forEach(game => {
            if (game.currentPlayer) {
                if (game.currentPlayer === userName) {
                    yourTurn.push(game)
                } else {
                    theirTurn.push(game);
                }
            } else {
                completedGames.push(game);
            }
        });

        // Order "your turn" games by oldest. Other people might be waiting for you.
        yourTurn.sort((a, b) => {
            // Games with no moves have a 'last move' value of 0 which will be ordered in the wrong
            // order so, in that case, flip the sort order.
            if (a.lastMoveAt.valueOf() === 0) {
                return 1;
            } else if (b.lastMoveAt.valueOf() === 0) {
                return -1;
            }

            return a.lastMoveAt - b.lastMoveAt;
        });

        // Order "their turn" games by time of last move descending.
        theirTurn.sort((a, b) => b.lastMoveAt - a.lastMoveAt);

        // Order completed games by time of completion descending.
        completedGames.sort((a, b) => b.lastMoveAt - a.lastMoveAt);

        this.setState({
            games: {
                yourTurn,
                theirTurn,
                completedGames,
            },
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
        return <GamesView games={this.state.games} userName={this.context.userName} />
    }
}

export default Games;
