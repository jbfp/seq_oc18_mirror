import PropTypes from 'prop-types';
import React from 'react';
import { Link } from 'react-router-dom';
import BoardView from './BoardView';
import { ServerContext } from "../contexts";
import DeckView from './DeckView';
import PlayerView from './PlayerView';
import PlayersView from './PlayersView';
import RulesView from './RulesView';
import './Game.css';

class GameView extends React.PureComponent {
    static contextType = ServerContext;

    static propTypes = {
        game: PropTypes.object,
        hideCards: PropTypes.bool.isRequired,
        onCardClick: PropTypes.func.isRequired,
        onCoordClick: PropTypes.func.isRequired,
        selectedCard: PropTypes.object,
    };

    render() {
        const { game, hideCards, onCardClick, onCoordClick, selectedCard } = this.props;
        let $body;

        if (game) {
            const playerObj = {
                hand: game.hand,
                handle: this.context.userName,
                isCurrentPlayer: game.currentPlayerId === game.playerId,
                team: game.team,
            };

            const latestMoveAt = game.moves.length > 0 ? game.moves[0].coord : null;

            $body = (
                <div>
                    <PlayersView currentPlayerId={game.currentPlayerId} players={game.players} winner={game.winner} />

                    <BoardView
                        board={game.board}
                        chips={game.chips}
                        highlightedCellValue={selectedCard}
                        latestMoveAt={latestMoveAt}
                        onCoordClick={onCoordClick}
                    />

                    <PlayerView hideCards={hideCards} onCardClick={onCardClick} selectedCard={selectedCard} {...playerObj} />

                    <div className="game-metadata">
                        <div>
                            <DeckView discards={game.discards} numberOfCardsInDeck={game.numberOfCardsInDeck} />
                        </div>

                        <div>
                            <RulesView {...game.rules} />
                        </div>
                    </div>
                </div>
            );
        } else {
            $body = (
                <div>Loading...</div>
            );
        }

        return (
            <div className="game">
                <Link to="/">Go back</Link>
                <hr />
                {$body}
            </div>
        )
    }
}

export default GameView;
