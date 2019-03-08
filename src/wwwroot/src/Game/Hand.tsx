import React from 'react';
import * as t from '../types';
import Card from './Card';

interface HandProps {
    cards: t.Card[];
    deadCards: t.Card[];
    hideCards: boolean;
    selectedCard: t.Card | null;
    onCardClick: (card: t.Card) => void;
}

export default function Hand(props: HandProps) {
    const { cards, deadCards, hideCards, onCardClick, selectedCard } = props;
    const classes = ['hand'];

    if (hideCards) {
        classes.push('hide');
    }

    return (
        <div className={classes.join(' ')}>
            {cards.map((card, idx) => {
                const key = `${card.deckNo}_${card.suit}_${card.rank}`;

                const isDead = deadCards.some(deadCard =>
                    deadCard.deckNo === card.deckNo &&
                    deadCard.suit === card.suit &&
                    deadCard.rank === card.rank);

                return (
                    <div key={key} className="hand-card">
                        <Card
                            card={card}
                            isDead={isDead}
                            isSelected={card === selectedCard}
                            onCardClick={onCardClick}
                        />

                        <kbd>{idx + 1}</kbd>
                    </div>
                );
            })}
        </div>
    )
}
