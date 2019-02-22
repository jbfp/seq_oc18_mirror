import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps } from "react-router";
import { ServerContext } from '../contexts';
import { Board, GameId, GameState, LoadGameResponseKind } from "../types";
import Game from './Game';

interface GameLoaderProps {
    id: GameId;
}

interface GameLoaderState {
    board: Board;
    game: GameState;
}

export default function GameLoader(props: RouteComponentProps<GameLoaderProps>) {
    const context = useContext(ServerContext);
    const [state, setState] = useState<GameLoaderState | null>(null);

    useEffect(() => {
        loadGameAsync();
    }, [props.match.params.id]);

    useEffect(() => {
        document.addEventListener('visibilitychange', handleVisibilityChange, false);
        return () => document.removeEventListener('visibilitychange', handleVisibilityChange, false);
    });

    async function loadGameAsync() {
        const gameId = props.match.params.id;
        const version = state ? state.game.index : null;
        const result = await context.getGameByIdAsync(gameId, version);

        if (result.kind === LoadGameResponseKind.Ok) {
            setState({ game: result.game, board: result.board });
        } else if (result.kind === LoadGameResponseKind.NotChanged) {
            return;
        } else if (result.kind === LoadGameResponseKind.NotFound) {
            setState(null);
        }
    }

    async function handleVisibilityChange(event: Event) {
        const $document = event.target as HTMLDocument;

        if ($document && $document.visibilityState === 'visible') {
            await loadGameAsync();
        }
    }

    async function handleRequestReload() {
        await loadGameAsync();
    }

    if (state) {
        return (
            <Game
                id={props.match.params.id}
                game={state.game}
                board={state.board}
                onRequestReload={handleRequestReload}
            />
        );
    } else {
        return <div>Loading...</div>;
    }
}
