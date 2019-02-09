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
        hideCards: PropTypes.bool.isRequired,
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
        const { hand, handle, hideCards, isCurrentPlayer, onCardClick, selectedCard, team } = this.props;

        return (
            <div className="player" data-team={team}>
                <Hand cards={hand} hideCards={hideCards} onCardClick={onCardClick} selectedCard={selectedCard} />

                <span className="player-name" data-current-player={isCurrentPlayer}>
                    {handle}
                </span>
            </div>
        );
    }
}

export default PlayerView;
