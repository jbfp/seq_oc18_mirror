import React, { useCallback, useContext, useEffect, useRef, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom';
import { ServerContext } from '../contexts';
import * as t from '../types';
import Game from './Game';
import PageVisibility from './page-visibility';
import { GameState, init, reducer } from './reducer';

interface GameLoaderProps {
    id: t.GameId;
}

export default function GameLoader(props: RouteComponentProps<GameLoaderProps>) {
    const gameId = props.match.params.id;
    const context = useContext(ServerContext);
    const [game, setState] = useState<GameState | null>(null);
    const [error, setError] = useState<string | null>(null);
    const timerHandle = useRef<number | undefined>(undefined);
    const timeouts = useRef<number>(0);

    const loadGameAsync = useCallback(async () => {
        const result = await context.getGameByIdAsync(gameId);

        if (result.kind === t.LoadGameResponseKind.Error) {
            timeouts.current++;
        } else {
            timeouts.current = 0;
        }

        setError(null);

        if (result.kind === t.LoadGameResponseKind.Ok) {
            const initialState = init(result.init, result.board);
            const finalState = result.updates.reduce(reducer, initialState);

            setState((currentState) => {
                if (currentState && currentState.version === finalState.version) {
                    return currentState;
                } else {
                    return finalState;
                }
            });
        } else if (result.kind === t.LoadGameResponseKind.NotFound) {
            setState(null);
        } else if (result.kind === t.LoadGameResponseKind.Error) {
            setError(result.error.message);
        }
    }, [gameId, context]);

    useEffect(() => {
        const timerHandler = async () => {
            await loadGameAsync();
            const delay = getNextDelay(0);
            timerHandle.current = window.setTimeout(timerHandler, delay);
        };

        timerHandler();

        return () => {
            window.clearTimeout(timerHandle.current);
        };
    }, [loadGameAsync]);

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
    }, [loadGameAsync]);

    const elements = [];

    if (error) {
        elements.push((
            <div key="error">
                Failed to load newest game state: {error}&nbsp;
                <button className="anchor" type="button" onClick={loadGameAsync}>
                    Click here to try again
                </button>.
                <br /><br />
            </div>
        ));
    }

    if (game) {
        elements.push((
            <Game
                key="game"
                id={gameId}
                init={game}
                onRequestReload={loadGameAsync}
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
