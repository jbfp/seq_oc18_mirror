import React from 'react';
import Hand from './Hand';
import { Card, PlayerHandle, Team } from "../types";

interface PlayerViewProps {
    hand: Card[];
    handle: PlayerHandle;
    hideCards: boolean;
    isCurrentPlayer: boolean;
    selectedCard: Card | null;
    team: Team;
    onCardClick: (card: Card) => void;
}

export default function PlayerView(props: PlayerViewProps) {
    const { hand, handle, hideCards, isCurrentPlayer, onCardClick, selectedCard, team } = props;

    return (
        <div className="player" data-team={team}>
            <Hand cards={hand} hideCards={hideCards} onCardClick={onCardClick} selectedCard={selectedCard} />

            <span className="player-name" data-current-player={isCurrentPlayer}>
                {handle}
            </span>
        </div>
    );
}
