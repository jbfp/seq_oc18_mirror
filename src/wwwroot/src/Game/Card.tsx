import React, { useCallback, useMemo } from 'react';
import * as t from "../types";

interface CardProps {
    card: t.Card;
    className?: string;
    isDead: boolean;
    isSelected: boolean;
    onCardClick: (card: t.Card) => void;
}

export default function Card(props: CardProps) {
    const { card, className, isDead, isSelected, onCardClick } = props;
    const classes = useMemo(() => ['card', className].join(' '), [className]);
    const handleCardClick = useCallback(
        () => onCardClick(card),
        [card, onCardClick]);

    return (
        <div
            className={classes}
            data-suit={card.suit}
            data-rank={card.rank}
            data-dead={isDead}
            data-selected={isSelected}
            onClick={handleCardClick}
        ></div>
    );
}

