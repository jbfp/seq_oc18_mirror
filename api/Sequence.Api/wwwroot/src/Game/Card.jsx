import React from 'react';

class Card extends React.PureComponent {
    render() {
        const { card, isSelected, onCardClick } = this.props;

        return (
            <div
                className="card"
                data-suit={card.suit}
                data-rank={card.rank}
                data-selected={isSelected}
                onClick={() => onCardClick(card)}
            ></div>
        );
    }
}

export default Card;
