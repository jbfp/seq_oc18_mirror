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

        // Order completed dates by time of completion descending.
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
