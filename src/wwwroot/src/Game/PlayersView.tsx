import React, { useCallback } from 'react';
import * as t from '../types';
import Card from './Card';

interface Player extends t.Player {
    latestCardPlayed: t.Card | null;
}

interface PlayersViewProps {
    currentPlayerId: t.PlayerId | null;
    players: Player[];
    winner: t.Team | null;
}

export default function PlayersView(props: PlayersViewProps) {
    const { currentPlayerId, players, winner } = props;
    const $players = players.map((player) => (
        <PlayerView
            key={player.id}
            {...player}
            isCurrentPlayer={player.id === currentPlayerId}
            isWinner={player.team === winner}
        />
    ));

    return (
        <div className="players">
            {$players}
        </div>
    );
}

interface PlayerViewProps {
    handle: t.PlayerHandle;
    isCurrentPlayer: boolean;
    isWinner: boolean;
    latestCardPlayed: t.Card | null;
    team: t.Team;
    type: t.PlayerType;
}

function PlayerView(props: PlayerViewProps) {
    const { handle, isCurrentPlayer, isWinner, latestCardPlayed, team } = props;
    const name = isWinner ? `👑 ${handle}` : handle;
    const handleCardClick = useCallback(() => undefined, []);
    const $latestCardPlayed = latestCardPlayed === null ? null : (
        <Card
            card={latestCardPlayed}
            className="small"
            isDead={false}
            isSelected={false}
            onCardClick={handleCardClick}
        />
    );

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
                {$latestCardPlayed}
            </div>
        </div>
    );
}
