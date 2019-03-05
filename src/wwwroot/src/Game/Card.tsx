import React from 'react';
import * as t from "../types";

interface CardProps {
    card: t.Card;
    isDead: boolean;
    isSelected: boolean;
    onCardClick: (card: t.Card) => void;
}

export default function Card(props: CardProps) {
    const { card, isDead, isSelected, onCardClick } = props;

    return (
        <div
            className="card"
            data-suit={card.suit}
            data-rank={card.rank}
            data-dead={isDead}
            data-selected={isSelected}
            onClick={() => onCardClick(card)}
        ></div>
    );
}

