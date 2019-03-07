import React, { useCallback } from 'react';
import * as t from '../types';
import Card from './Card';

interface Player extends t.Player {
    lastMove: t.Move | null;
}

interface PlayersViewProps {
    currentPlayerId: t.PlayerId | null;
    players: Player[];
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
    lastMove: t.Move | null;
    team: t.Team;
    type: t.PlayerType;
}

function PlayerView(props: PlayerViewProps) {
    const { handle, isCurrentPlayer, isWinner, lastMove, team } = props;
    const name = isWinner ? `👑 ${handle}` : handle;
    const handleCardClick = useCallback(() => { }, []);

    return (
        <div
            className="players-player"
            data-team={team}
            data-current-player={isCurrentPlayer}
            data-winner={isWinner}
            title={name}
        >
            <div className="players-player-name">
                {name}
            </div>

            <div>
                {lastMove === null ? null : (<Card
                    card={lastMove.cardUsed}
                    className="small"
                    isDead={false}
                    isSelected={false}
                    onCardClick={handleCardClick}
                />)}
            </div>
        </div>
    );
}
