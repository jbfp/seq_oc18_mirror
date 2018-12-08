import PropTypes from 'prop-types';
import React from 'react';
import { Link } from 'react-router-dom';
import BoardView from './BoardView';
import DeckView from './DeckView';
import OpponentView from './OpponentView';
import PlayerView from './PlayerView';
import './Game.css';

class GameView extends React.PureComponent {
    static propTypes = {
        game: PropTypes.object,
        onCardClick: PropTypes.func.isRequired,
        onCoordClick: PropTypes.func.isRequired,
        playerId: PropTypes.string.isRequired,
        selectedCard: PropTypes.object,
    };

    render() {
        const { game, onCardClick, onCoordClick, playerId, selectedCard } = this.props;
        let $body;

        if (game) {
            const opponentObj = {
                ...game.players.find(p => p.id !== playerId),
            };

            opponentObj.isCurrentPlayer = game.currentPlayerId === opponentObj.id;

            const playerObj = {
                hand: game.hand,
                id: playerId,
                isCurrentPlayer: game.currentPlayerId === playerId,
                team: game.team,
            };

            $body = (
                <div>
                    <Link to="/">Go back</Link>
                    <hr />
                    <OpponentView {...opponentObj} />
                    <BoardView board={game.board} chips={game.chips} onCoordClick={onCoordClick} />
                    <PlayerView onCardClick={onCardClick} selectedCard={selectedCard} {...playerObj} />
                    <DeckView discards={game.discards} numberOfCardsInDeck={game.numberOfCardsInDeck} />
                </div>
            );
        } else {
            $body = (
                <div>Loading...</div>
            );
        }

        return (
            <div className="game">
                {$body}
            </div>
        )
    }
}

export default GameView;
