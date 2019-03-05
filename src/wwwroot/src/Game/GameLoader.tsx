import React, { useCallback, useContext, useEffect, useRef, useState } from 'react';
import { RouteComponentProps } from "react-router";
import { Link } from 'react-router-dom';
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
    const [error, setError] = useState<string | null>(null);
    const timerHandle = useRef<number | undefined>(undefined);
    const timeouts = useRef<number>(0);

    useEffect(() => {
        const timerHandler = async () => {
            await loadGameAsync();
            const delay = getNextDelay(timeouts.current);
            timerHandle.current = window.setTimeout(timerHandler, delay);
        };

        timerHandler();

        return () => {
            window.clearTimeout(timerHandle.current);
        };
    }, []);

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
    }, []);

    async function loadGameAsync() {
        const gameId = props.match.params.id;
        const version = state ? state.game.index : null;
        const result = await context.getGameByIdAsync(gameId, version);

        if (result.kind === LoadGameResponseKind.Error) {
            timeouts.current++;
        } else {
            timeouts.current = 0;
        }

        setError(null);

        if (result.kind === LoadGameResponseKind.Ok) {
            setState({ game: result.game, board: result.board });
        } else if (result.kind === LoadGameResponseKind.NotChanged) {
            // no-op.
        } else if (result.kind === LoadGameResponseKind.NotFound) {
            setState(null);
        } else if (result.kind === LoadGameResponseKind.Error) {
            setError(result.error.message);
        }
    }

    const handleRequestReload = useCallback(async () => {
        await loadGameAsync();
    }, []);

    const elements = [];

    if (error) {
        elements.push((
            <div key="error">
                Failed to load newest game state: {error}&nbsp;
                <a href="#" onClick={loadGameAsync}>Click here to try again</a>.
                <br /><br />
            </div>
        ));
    }

    if (state) {
        elements.push((
            <Game
                key="game"
                id={props.match.params.id}
                game={state.game}
                board={state.board}
                onRequestReload={handleRequestReload}
            />
        ));
    }

    if (elements.length === 0) {
        elements.push((
            <div key="loading">
                Loading...
            </div>
        ));
    }

    return (
        <div className="game">
            <Link to="/">Go back</Link>
            <hr />
            {elements}
        </div>
    );
}

function getNextDelay(numFailures: number): number {
    const base = 5000;
    const current = Math.max(0, numFailures - 1);
    const delta = (Math.pow(2, current) - 1) * (1.0 + Math.random()) * 0.1;
    return Math.min(base + 1000 * delta, 30000);
}
