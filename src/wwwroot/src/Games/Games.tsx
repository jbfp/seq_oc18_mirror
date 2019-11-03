import React, { useContext, useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from '../contexts';
import * as t from '../types';
import GamesView from './GamesView';
import { Game, GameCollections } from './types';

export default function Games() {
    const context = useContext(ServerContext);
    const [games, setGames] = useState<GameCollections | null>(null);
    const [simulations, setSimulations] = useState<t.GameId[] | null>(null);
    const timerHandle = useRef<number | undefined>(undefined);
    const userName = context.userName;

    useEffect(() => {
        const timerHandler = async () => {
            const promise0 = context
                .getGamesAsync()
                .then((allGames) => mapAllGamesToCollection(allGames, userName))
                .then(setGames);

            const promise1 = context
                .getSimulationsAsync()
                .then(setSimulations);

            await Promise.all([promise0, promise1]);

            timerHandle.current = window.setTimeout(timerHandler, 10000);
        };

        timerHandler();

        return () => {
            window.clearTimeout(timerHandle.current);
        };
    }, [context, userName]);

    const $elements = [];

    if (games) {
        $elements.push((
            <GamesView
                key="games"
                games={games}
                userName={context.userName}
            />
        ));
    }

    if (simulations) {
        $elements.push((
            <div key="simulations">
                <h2>Simulations</h2>

                {simulations.length > 0 ?
                    (
                        <ul>
                            {simulations.map((sim) => (
                                <li key={sim}>
                                    <Link to={`/games/${sim}`}>
                                        {sim}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    ) :
                    (
                        <div key="simulations" className="game-list-empty">
                            No simulations found
                        </div>
                    )}
            </div>
        ));
    }

    if ($elements.length > 0) {
        return (<>{$elements}</>);
    } else {
        return (
            <div>
                <p>Loading games...</p>
            </div>
        );
    }
}

function mapAllGamesToCollection(allGames: Game[], userName: string): GameCollections {
    const completedGames: Game[] = [];
    const yourTurn: Game[] = [];
    const theirTurn: Game[] = [];

    allGames.map((game) => {
        return {
            ...game,
            lastMoveAt: new Date(game.lastMoveAt),
        };
    }).forEach((game) => {
        if (game.currentPlayer) {
            if (game.currentPlayer === userName) {
                yourTurn.push(game);
            } else {
                theirTurn.push(game);
            }
        } else {
            completedGames.push(game);
        }
    });

    // Order "your turn" games by oldest. Other people might be waiting for you.
    yourTurn.sort((a, b) => {
        // Games with no moves have a 'last move' value of 0 which will be ordered in the wrong
        // order so, in that case, flip the sort order.
        if (a.lastMoveAt.valueOf() === 0) {
            return 1;
        } else if (b.lastMoveAt.valueOf() === 0) {
            return -1;
        }

        return a.lastMoveAt.valueOf() - b.lastMoveAt.valueOf();
    });

    // Order "their turn" games by time of last move descending.
    theirTurn.sort((a, b) => b.lastMoveAt.valueOf() - a.lastMoveAt.valueOf());

    // Order completed games by time of completion descending.
    completedGames.sort((a, b) => b.lastMoveAt.valueOf() - a.lastMoveAt.valueOf());

    return { completedGames, theirTurn, yourTurn };
}
