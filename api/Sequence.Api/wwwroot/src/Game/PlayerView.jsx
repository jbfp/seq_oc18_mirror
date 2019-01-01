import PropTypes from 'prop-types';
import React from 'react';
import Hand from './Hand';

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

        return (
            <div className="player" data-team={team}>
                <span className="player-name" data-current-player={isCurrentPlayer}>
                    {handle}
                </span>

                <Hand cards={hand} onCardClick={onCardClick} selectedCard={selectedCard} />
            </div>
        );
    }
}

export default PlayerView;
