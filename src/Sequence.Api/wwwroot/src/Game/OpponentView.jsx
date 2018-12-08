import PropTypes from 'prop-types';
import React from 'react';

class OpponentView extends React.PureComponent {
    static propTypes = {
        id: PropTypes.string.isRequired,
        isCurrentPlayer: PropTypes.bool.isRequired,
        numberOfCards: PropTypes.number.isRequired,
        team: PropTypes.oneOf(['red', 'green']).isRequired,
    };

    render() {
        const { id, isCurrentPlayer, numberOfCards, team } = this.props;

        const Card = () => <div className="card card-back"></div>;
        const Hand = () => Array(numberOfCards).fill().map((_, idx) => <Card key={idx} />);

        return (
            <div className="player opponent" data-team={team}>
                <span className="player-name" data-current-player={isCurrentPlayer}>
                    {id}
                </span>

                <div className="hand opponent-hand">
                    <Hand />
                </div>
            </div>
        );
    }
}

export default OpponentView;
