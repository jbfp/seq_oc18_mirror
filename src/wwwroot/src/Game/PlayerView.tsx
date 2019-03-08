import React, { useCallback } from 'react';
import Hand from './Hand';
import { Card, PlayerHandle, Team } from "../types";

interface PlayerViewProps {
    deadCards: Card[];
    hand: Card[];
    handle: PlayerHandle;
    hasExchangedDeadCard: boolean;
    hideCards: boolean;
    isCurrentPlayer: boolean;
    selectedCard: Card | null;
    team: Team;
    onCardClick: (card: Card) => void;
    onExchangeDeadCardClick: () => void;
}

export default function PlayerView(props: PlayerViewProps) {
    const { deadCards, hand, hasExchangedDeadCard, hideCards, selectedCard, team } = props;
    const { onCardClick, onExchangeDeadCardClick } = props;
    const isDead = selectedCard && deadCards.some(deadCard =>
        deadCard.deckNo === selectedCard.deckNo &&
        deadCard.suit === selectedCard.suit &&
        deadCard.rank === selectedCard.rank);;

    const handleExchangeDeadCardClick = useCallback((event: React.MouseEvent) => {
        event.preventDefault();
        onExchangeDeadCardClick();
    }, []);

    return (
        <div className="player" data-team={team}>
            <Hand
                cards={hand}
                deadCards={deadCards}
                hideCards={hideCards}
                onCardClick={onCardClick}
                selectedCard={selectedCard}
            />

            {isDead && !hasExchangedDeadCard ? (
                <a className="exchange-dead-card-btn" href="#" onClick={handleExchangeDeadCardClick}>
                    &gt;&nbsp;Exchange dead card&nbsp;&lt;
                </a>
            ) : null}
        </div >
    );
}
