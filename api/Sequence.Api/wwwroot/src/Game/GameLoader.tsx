import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from "react-router";
import { ServerContext } from '../contexts';
import { Board, GameId, GameState } from "../types";
import Game from './Game';

interface GameLoaderProps {
    id: GameId;
}

export default function GameLoader(props: RouteComponentProps<GameLoaderProps>) {
    const context = useContext(ServerContext);
    const [game, setGame] = useState<GameState | null>(null);
    const [board, setBoard] = useState<Board | null>(null);

    async function loadGameAsync() {
        const gameId = props.match.params.id;
        const version = game ? game.index : null;
        const gameState = await context.getGameByIdAsync(gameId, version);
        setGame(gameState);

        if (gameState === null) {
            return;
        }

        const board = await context.getBoardAsync(gameState.rules.boardType);
        setBoard(board);
    }

    async function handleVisibilityChange(event: Event) {
        const $document = event.target as HTMLDocument;

        if ($document && !$document.hidden) {
            await loadGameAsync();
        }
    }

    useEffect(() => {
        loadGameAsync();
    }, [props.match.params.id]);

    useEffect(() => {
        document.addEventListener('visibilitychange', handleVisibilityChange, false);
        return () => document.removeEventListener('visibilitychange', handleVisibilityChange, false);
    }, []);

    if (game && board) {
        return (
            <Game
                id={props.match.params.id}
                game={game}
                board={board}
                onRequestReload={() => loadGameAsync()}
            />
        );
    } else {
        return <div>Loading...</div>;
    }
}
