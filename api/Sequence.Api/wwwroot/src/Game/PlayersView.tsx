import React from 'react';
import * as t from '../types';

interface PlayersViewProps {
    currentPlayerId: t.PlayerId;
    players: t.Player[];
    winner: t.Team | null;
}

export default function PlayersView(props: PlayersViewProps) {
    const { currentPlayerId, players, winner } = props;

    return (
        <div className="players">
            {players.map(player => (
                <PlayerView
                    key={player.id}
                    {...player}
                    isCurrentPlayer={player.id === currentPlayerId}
                    isWinner={player.team === winner}
                />
            ))}
        </div>
    );
}

interface PlayerViewProps {
    handle: t.PlayerHandle;
    isCurrentPlayer: boolean;
    isWinner: boolean;
    team: t.Team;
    type: t.PlayerType;
}

function PlayerView(props: PlayerViewProps) {
    const { handle, isCurrentPlayer, isWinner, team, type } = props;
    const name = type === t.PlayerType.Bot ? `🤖 ${handle} 🤖` : handle;

    return (
        <div
            className="players-player"
            data-team={team}
            data-current-player={isCurrentPlayer}
            data-winner={isWinner}
            title={name}
        >
            <div>
                {name}
            </div>
        </div>
    );
}
