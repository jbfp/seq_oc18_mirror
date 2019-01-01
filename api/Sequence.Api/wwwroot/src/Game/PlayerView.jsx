import PropTypes from 'prop-types';
import React from 'react';

class PlayerView extends React.PureComponent {
    static propTypes = {
        hand: PropTypes.arrayOf(PropTypes.shape({
            deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
            suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
            rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
        })).isRequired,
        handle: PropTypes.string.isRequired,
        isCurrentPlayer: PropTypes.bool.isRequired,
        onCardClick: PropTypes.func.isRequired,
        selectedCard: PropTypes.shape({
            deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
            suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
            rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
        }),
        team: PropTypes.oneOf(['red', 'green', 'blue']).isRequired,
    };

    render() {
        const { hand, handle, isCurrentPlayer, onCardClick, selectedCard, team } = this.props;

        const Card = ({ card }) => (
            <div
                className="card"
                data-suit={card.suit}
                data-rank={card.rank}
                data-selected={card === selectedCard}
                onClick={() => onCardClick(card)}
            ></div>
        );

        const Hand = () => hand.map((card, idx) => {
            const key = `${card.deckNo}_${card.suit}_${card.rank}`;

            return (
                <div key={key} className="hand-card">
                    <Card card={card} />
                    <kbd>{idx + 1}</kbd>
                </div>
            );
        });

        return (
            <div className="player" data-team={team}>
                <span className="player-name" data-current-player={isCurrentPlayer}>
                    {handle}
                </span>

                <div className="hand">
                    <Hand />
                </div>
            </div>
        );
    }
}

export default PlayerView;
