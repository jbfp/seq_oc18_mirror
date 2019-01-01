import React from 'react';
import Card from './Card';

class Hand extends React.PureComponent {
    render() {
        const { cards, onCardClick, selectedCard } = this.props;

        return (
            <div className="hand">
                {cards.map((card, idx) => {
                    const key = `${card.deckNo}_${card.suit}_${card.rank}`;

                    return (
                        <div key={key} className="hand-card">
                            <Card
                                card={card}
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
}

export default Hand;
