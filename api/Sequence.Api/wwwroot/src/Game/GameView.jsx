import PropTypes from 'prop-types';
import React from 'react';
import { Link } from 'react-router-dom';
import BoardView from './BoardView';
import { ServerContext } from "../contexts";
import DeckView from './DeckView';
import PlayerView from './PlayerView';
import PlayersView from './PlayersView';
import './Game.css';

class GameView extends React.PureComponent {
    static contextType = ServerContext;

    static propTypes = {
        game: PropTypes.object,
        onCardClick: PropTypes.func.isRequired,
        onCoordClick: PropTypes.func.isRequired,
        selectedCard: PropTypes.object,
    };

    render() {
        const { game, onCardClick, onCoordClick, selectedCard } = this.props;
        let $body;

        if (game) {
            const playerObj = {
                hand: game.hand,
                handle: this.context.userName,
                isCurrentPlayer: game.currentPlayerId === game.playerId,
                team: game.team,
            };

            $body = (
                <div>
                    <Link to="/">Go back</Link>
                    <hr />
                    <PlayersView currentPlayerId={game.currentPlayerId} players={game.players} winner={game.winner} />
                    <BoardView board={game.board} chips={game.chips} onCoordClick={onCoordClick} highlightedCellValue={selectedCard} />
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
