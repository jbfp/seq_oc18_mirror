import React, { useMemo } from 'react';
import { Link } from "react-router-dom";
import { GameState } from './reducer';
import { cardKey } from "./helpers";
import * as t from '../types';
import BoardView from './BoardView';
import DeckView from './DeckView';
import PlayerView from './PlayerView';
import PlayersView from './PlayersView';
import RulesView from './RulesView';
import './Game.css';

interface GameViewProps {
    game: GameState;
    selectedCard: t.Card | null;
    onCardClick: (card: t.Card) => void;
    onCoordClick: (coord: t.Coord) => void;
    onExchangeDeadCardClick: () => void;
}

export default function GameView(props: GameViewProps) {
    const { game, selectedCard } = props;
    const { onCardClick, onCoordClick, onExchangeDeadCardClick } = props;

    let reCreateLink = null;

    if (game.winnerTeam) {
        const players = game.players;
        const numPlayers = game.players.length;
        const startIndex = players.findIndex(player => player.id === game.playerId);

        const query = new URLSearchParams();
        query.append('board-type', game.boardType);
        query.append('win-condition', game.winCondition.toString());
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
            latestCardPlayed: game.latestCardPlayed.get(player.id) || null,
        };
    });

    // Shift players array so that the player is always the top player in the opponent list.
    const playerIdx = players.findIndex(player => player.id === game.playerId);
    const shiftedPlayers = playerIdx !== -1 ? [
        players[playerIdx],
        ...players.slice(playerIdx + 1),
        ...players.slice(0, playerIdx),
    ] : players;

    const hand = useMemo(() => {
        if (game.hand) {
            return Array.from(game.hand.values());
        }

        return null;
    }, [game.hand]);

    const selectedCardKey = useMemo(() => {
        if (selectedCard) {
            return cardKey(selectedCard);
        }

        return null;
    }, [selectedCard]);

    return (
        <div>
            {reCreateLink}

            <PlayersView
                currentPlayerId={game.currentPlayerId}
                players={shiftedPlayers}
                winner={game.winnerTeam} />

            <BoardView
                board={game.board}
                chips={game.chips}
                highlightedCellValue={selectedCard}
                latestMoveAt={game.latestMoveAt}
                onCoordClick={onCoordClick}
            />

            {game.playerId ? (
                <PlayerView
                    deadCards={game.deadCards}
                    hasExchangedDeadCard={game.hasExchangedDeadCard}
                    hand={hand!}
                    isCurrentPlayer={game.currentPlayerId === game.playerId}
                    onCardClick={onCardClick}
                    onExchangeDeadCardClick={onExchangeDeadCardClick}
                    selectedCardKey={selectedCardKey}
                    team={game.playerTeam!}
                />
            ) : null}

            <div className="game-metadata">
                <div>
                    <DeckView numberOfCardsInDeck={game.numCardsInDeck} />
                </div>

                <div>
                    <RulesView
                        boardType={t.BoardType.OneEyedJack}
                        firstPlayer={game.firstPlayer}
                        winCondition={game.winCondition}
                    />
                </div>
            </div>
        </div>
    );
}
