import React from 'react';

interface DeckViewProps {
    numberOfCardsInDeck: number;
}

export default function DeckView(props: DeckViewProps) {
    const { numberOfCardsInDeck } = props;

    return (
        <div className="deck card card-back">
            {numberOfCardsInDeck}
        </div>
    );
}
