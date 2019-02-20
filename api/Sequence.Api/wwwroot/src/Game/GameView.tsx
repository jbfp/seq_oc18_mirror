import React from 'react';
import { Link } from 'react-router-dom';
import * as t from '../types';
import BoardView from './BoardView';
import DeckView from './DeckView';
import PlayerView from './PlayerView';
import PlayersView from './PlayersView';
import RulesView from './RulesView';
import './Game.css';

interface GameViewProps {
    board: t.Board;
    game: t.GameState;
    hideCards: boolean;
    selectedCard: t.Card | null;
    userName: string;
    onCardClick: (card: t.Card) => void;
    onCoordClick: (coord: t.Coord) => void;
}

export default function GameView(props: GameViewProps) {
    const { board, game, hideCards, selectedCard, userName } = props;
    const { onCardClick, onCoordClick } = props;

    const playerObj = {
        hand: game.hand,
        handle: userName,
        isCurrentPlayer: game.currentPlayerId === game.playerId,
        team: game.team,
    };

    const latestMoveAt = game.moves.length > 0
        ? game.moves[0].coord
        : null;

    return (
        <div className="game">
            <Link to="/">Go back</Link>
            <hr />
            <div>
                <PlayersView currentPlayerId={game.currentPlayerId} players={game.players} winner={game.winner} />

                <BoardView
                    board={board}
                    chips={game.chips}
                    highlightedCellValue={selectedCard}
                    latestMoveAt={latestMoveAt}
                    onCoordClick={onCoordClick}
                />

                <PlayerView hideCards={hideCards} onCardClick={onCardClick} selectedCard={selectedCard} {...playerObj} />

                <div className="game-metadata">
                    <div>
                        <DeckView numberOfCardsInDeck={game.numberOfCardsInDeck} />
                    </div>

                    <div>
                        <RulesView {...game.rules} />
                    </div>
                </div>
            </div>
        </div>
    );
}
