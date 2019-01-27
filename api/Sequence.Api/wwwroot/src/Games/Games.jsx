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
        return <GamesView games={this.state.games} userName={this.context.userName} />
    }
}

export default Games;