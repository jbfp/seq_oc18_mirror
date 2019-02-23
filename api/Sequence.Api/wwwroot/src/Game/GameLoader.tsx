import React, { useContext, useEffect, useRef, useState } from 'react';
import { RouteComponentProps } from "react-router";
import { ServerContext } from '../contexts';
import { Board, GameId, GameState, LoadGameResponseKind } from "../types";
import PageVisibility from './page-visibility';
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
    const timerHandle = useRef<number | undefined>(undefined);

    useEffect(() => {
        loadGameAsync();
    }, [props.match.params.id]);

    useEffect(() => {
        const DEFAULT_TIMEOUT = 5000;

        const timerHandler = async () => {
            await loadGameAsync();
            timerHandle.current = window.setTimeout(timerHandler, DEFAULT_TIMEOUT);
        };

        timerHandler();

        return () => {
            window.clearInterval(timerHandle.current);
        };
    });

    useEffect(() => {
        if (PageVisibility) {
            const { hidden, visibilityChange } = PageVisibility;

            const eventHandler = async () => {
                const document = window.document as any;

                if (document[hidden] === false) {
                    await loadGameAsync();
                }
            };

            document.addEventListener(visibilityChange, eventHandler, false);

            return () => {
                document.removeEventListener(visibilityChange, eventHandler, false);
            };
        }
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
