import React from 'react';

class Player extends React.PureComponent {
    render() {
        const { handle, isCurrentPlayer, team, type, isWinner } = this.props;
        const name = type === 'bot' ? `🤖 ${handle} 🤖` : handle;

        return (
            <div
                className="players-player"
                data-team={team}
                data-current-player={isCurrentPlayer}
                data-winner={isWinner}
            >
                <div>
                    {name}
                </div>
            </div>
        );
    }
}

class PlayersView extends React.PureComponent {
    render() {
        const { currentPlayerId, players, winner } = this.props;

        return (
            <div className="players">
                {players.map(player => (
                    <Player
                        key={player.id}
                        {...player}
                        isCurrentPlayer={player.id === currentPlayerId}
                        isWinner={player.team === (winner || {}).team} />
                ))}
            </div>
        );
    }
}

export default PlayersView;
