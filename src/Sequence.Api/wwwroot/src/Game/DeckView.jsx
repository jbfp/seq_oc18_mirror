import PropTypes from 'prop-types';
import React from 'react';

class DeckView extends React.PureComponent {
    static propTypes = {
        discards: PropTypes.arrayOf(PropTypes.shape({
            deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
            suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
            rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
        })).isRequired,
        numberOfCardsInDeck: PropTypes.number.isRequired,
    };

    render() {
        const { numberOfCardsInDeck } = this.props;

        return (
            <div className="deck card card-back" title={`${numberOfCardsInDeck} cards remain in the deck.`}>
                {numberOfCardsInDeck}
            </div>
        );
    }
}

export default DeckView;
