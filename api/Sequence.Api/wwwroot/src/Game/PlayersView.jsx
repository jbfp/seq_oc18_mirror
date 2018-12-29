import React from 'react';

class PlayersView extends React.PureComponent {
    render() {
        const { players, winner } = this.props;

        const $players = players.map((player, i) => {
            const handle = player.type === 'bot' ? `🤖 ${player.handle} 🤖` : player.handle;

            return (
                <div
                    key={i}
                    className="players-player"
                    data-team={player.team}
                    data-current-player={player.isCurrentPlayer}
                    data-winner={player.team === (winner || {}).team}
                >
                    <div>
                        {handle}
                    </div>
                </div>
            );
        });

        return (
            <div className="players">
                {$players}
            </div>
        );
    }
}

export default PlayersView;
