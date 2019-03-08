import React from 'react';
import { Link } from "react-router-dom";
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
    onExchangeDeadCardClick: () => void;
}

export default function GameView(props: GameViewProps) {
    const { board, game, hideCards, selectedCard, userName } = props;
    const { onCardClick, onCoordClick, onExchangeDeadCardClick } = props;

    const playerObj = {
        hand: game.hand,
        handle: userName,
        isCurrentPlayer: game.currentPlayerId === game.playerId,
        team: game.team,
    };

    const latestMoveAt = game.moves.length > 0
        ? game.moves[game.moves.length - 1].coord
        : null;

    let reCreateLink = null;

    if (game.winner) {
        const players = game.players;
        const numPlayers = game.players.length;
        const startIndex = players.findIndex(player => player.id === game.playerId);

        const query = new URLSearchParams();
        query.append('board-type', game.rules.boardType);
        query.append('win-condition', game.rules.winCondition.toString());
        query.append('num-players', numPlayers.toString());

        for (let i = 0; i < numPlayers - 1; i++) {
            const player = players[(startIndex + i + 1) % numPlayers];
            query.append('opponent-name', player.handle);
            query.append('opponent-type', player.type);
        }

        const url = `/new-game?${query.toString()}`;

        reCreateLink = (
            <div className="re-create-game">
                <Link to={url}>Re-create game</Link>
            </div>
        );
    }

    const players = game.players.map(player => {
        return {
            ...player,
            lastMove: game.moves
                .filter(move => move.byPlayerId === player.id)
                .reverse()
                .shift() || null,
        };
    });

    return (
        <div>
            {reCreateLink}

            <PlayersView currentPlayerId={game.currentPlayerId} players={players} winner={game.winner} />

            <BoardView
                board={board}
                chips={game.chips}
                highlightedCellValue={selectedCard}
                latestMoveAt={latestMoveAt}
                onCoordClick={onCoordClick}
            />

            <PlayerView
                deadCards={game.deadCards}
                hideCards={hideCards}
                onCardClick={onCardClick}
                onExchangeDeadCardClick={onExchangeDeadCardClick}
                selectedCard={selectedCard}
                {...playerObj}
            />

            <div className="game-metadata">
                <div>
                    <DeckView numberOfCardsInDeck={game.numberOfCardsInDeck} />
                </div>

                <div>
                    <RulesView {...game.rules} />
                </div>
            </div>
        </div>
    );
}
