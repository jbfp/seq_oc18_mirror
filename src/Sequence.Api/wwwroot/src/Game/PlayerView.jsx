import PropTypes from 'prop-types';
import React from 'react';

class PlayerView extends React.PureComponent {
    static propTypes = {
        hand: PropTypes.arrayOf(PropTypes.shape({
            deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
            suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
            rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
        })).isRequired,
        id: PropTypes.string.isRequired,
        isCurrentPlayer: PropTypes.bool.isRequired,
        onCardClick: PropTypes.func.isRequired,
        selectedCard: PropTypes.shape({
            deckNo: PropTypes.oneOf(['one', 'two']).isRequired,
            suit: PropTypes.oneOf(['hearts', 'spades', 'diamonds', 'clubs']).isRequired,
            rank: PropTypes.oneOf(['ace', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine', 'ten', 'jack', 'queen', 'king']).isRequired,
        }),
        team: PropTypes.oneOf(['red', 'green']).isRequired,
    };

    render() {
        const { hand, id, isCurrentPlayer, onCardClick, selectedCard, team } = this.props;

        const Card = ({ card }) => {
            return (
                <div
                    className="card"
                    data-suit={card.suit}
                    data-rank={card.rank}
                    data-selected={card === selectedCard}
                    onClick={() => onCardClick(card)}
                ></div>
            );
        };

        const Hand = () => hand.map((card, idx) => <Card key={idx} card={card} />);

        return (
            <div className="player user" data-team={team}>
                <span className="player-name" data-current-player={isCurrentPlayer}>
                    {id}
                </span>

                <div className="hand user-hand">
                    <Hand />
                </div>
            </div>
        );
    }
}

export default PlayerView;
