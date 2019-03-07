import React from 'react';
import Hand from './Hand';
import { Card, PlayerHandle, Team } from "../types";

interface PlayerViewProps {
    deadCards: number[];
    hand: Card[];
    handle: PlayerHandle;
    hideCards: boolean;
    isCurrentPlayer: boolean;
    selectedCard: Card | null;
    team: Team;
    onCardClick: (card: Card) => void;
}

export default function PlayerView(props: PlayerViewProps) {
    const { deadCards, hand, hideCards, selectedCard, team } = props;
    const { onCardClick } = props;

    return (
        <div className="player" data-team={team}>
            <Hand
                cards={hand}
                deadCards={deadCards}
                hideCards={hideCards}
                onCardClick={onCardClick}
                selectedCard={selectedCard}
            />
        </div>
    );
}
