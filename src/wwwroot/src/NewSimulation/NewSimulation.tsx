import { History } from 'history';
import React, { useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from '../contexts';
import { BoardType } from '../types';
import { GameSize } from './types';

interface NewSimulationProps {
    history: History;
}

export default function NewSimulation(props: NewSimulationProps) {
    const server = useContext(ServerContext);
    const [botTypes, setBotTypes] = useState<string[]>([]);
    const [players, setPlayers] = useState<string[]>([]);
    const [randomFirstPlayer, setRandomFirstPlayer] = useState<boolean>(true);
    const [boardType, setBoardType] = useState<BoardType>(BoardType.Sequence);
    const [winCondition, setWinCondition] = useState<number>(2);
    const [error, setError] = useState<string>('');
    const [busy, setBusy] = useState<boolean>(false);

    useEffect(() => {
        server.getBotsAsync().then(setBotTypes);
    }, [server]);

    const disabled = useMemo(() => {
        return players.length === 0
            || players.some((player) => player.length === 0)
            || boardType === null
            || winCondition === 0
            || busy;

    }, [boardType, busy, players, winCondition]);

    const handleNumPlayersChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        const numPlayers = Number.parseInt(event.target.value, 10);
        const players = new Array<string>(numPlayers).fill(botTypes[0]);
        setPlayers(players);
    }, [botTypes]);

    const handlePlayerChange = useCallback((i: number) => {
        return (event: React.ChangeEvent<HTMLSelectElement>) => {
            const value = event.target.value;

            setPlayers((ps) => {
                return [
                    ...ps.slice(0, i),
                    value,
                    ...ps.slice(i + 1),
                ];
            });
        };
    }, []);

    const handleRandomFirstPlayerChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setRandomFirstPlayer(event.target.checked);
    }, []);

    const handleBoardTypeChange = useCallback((newBoardType: BoardType) => {
        return () => setBoardType(newBoardType);
    }, []);

    const handleWinConditionChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        setWinCondition(event.target.valueAsNumber);
    }, []);

    const handleFocus = useCallback((event: React.FocusEvent<HTMLInputElement>) => {
        event.target.select();
    }, []);

    const submitAsync = useCallback(async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setBusy(true);
        setError('');

        let gameId;

        try {
            gameId = await server.createSimulationAsync({
                boardType,
                bots: players,
                numSequencesToWin: winCondition,
                randomFirstPlayer,
                seed: Math.trunc(Math.random() * 2147483647),
            });
        } catch (e) {
            setError(e.toString());
        } finally {
            setBusy(false);
        }

        if (gameId) {
            props.history.push(`/games/${gameId}`);
        }
    }, [boardType, randomFirstPlayer, players, props.history, server, winCondition]);

    return (
        <div>
            <p>
                Start a new simulation or click <Link to="/">here</Link> to go back.
            </p>

            <form onSubmit={submitAsync}>
                <div>
                    {GameSizes.map(([gameSize, name]) => (
                        <label key={gameSize} className="new-game-game-size">
                            <input
                                type="radio"
                                name="num-players"
                                readOnly={disabled}
                                value={gameSize}
                                onChange={handleNumPlayersChange}
                            />

                            <span className="new-game-game-size-text">
                                {name}
                            </span>
                        </label>
                    ))}
                </div>

                <div>
                    {players.map((type, i) => (
                        <div className="new-game-opponent" key={i}>
                            <select
                                value={type}
                                onChange={handlePlayerChange(i)}
                                autoFocus={true}
                            >
                                <option value="">Select bot type</option>
                                {botTypes.map((botType) => (
                                    <option key={botType} value={botType}>{botType}</option>
                                ))}
                            </select>
                        </div>
                    ))}
                </div>

                <label>
                    <input
                        type="checkbox"
                        checked={randomFirstPlayer}
                        onChange={handleRandomFirstPlayerChange}
                    /> Random first player
                </label>

                <div className="new-game-board-types">
                    <strong>Board:</strong>

                    <label className="new-game-board-types-type">
                        <input
                            type="radio"
                            name="board-type"
                            value={BoardType.OneEyedJack}
                            checked={boardType === BoardType.OneEyedJack}
                            onChange={handleBoardTypeChange(BoardType.OneEyedJack)}
                            readOnly={disabled}
                        />

                        <span>One-Eyed Jack</span>
                    </label>

                    <label className="new-game-board-types-type">
                        <input
                            type="radio"
                            name="board-type"
                            value={BoardType.Sequence}
                            checked={boardType === BoardType.Sequence}
                            onChange={handleBoardTypeChange(BoardType.Sequence)}
                            readOnly={disabled}
                        />

                        <span>Sequence®</span>
                    </label>
                </div>

                <div className="new-game-win-condition">
                    <label className="new-game-win-condition-container">
                        <strong className="new-game-win-condition-container-text">
                            Win condition (no. of sequences):
                        </strong>

                        <div className="new-game-win-condition-input-container">
                            <input
                                className="new-game-win-condition-input"
                                type="number"
                                inputMode="numeric"
                                pattern="[1-4]*"
                                min={1}
                                max={4}
                                value={winCondition || ''}
                                onFocus={handleFocus}
                                onChange={handleWinConditionChange}
                                readOnly={disabled}
                            />
                        </div>
                    </label>
                </div>

                <button className="new-game-submit" type="submit" disabled={disabled}>
                    Start simulation
                </button>

                <div>
                    <span className="new-game-error">
                        {error}
                    </span>
                </div>
            </form>
        </div>
    );
}

const GameSizes: Array<[GameSize, string]> = [
    [GameSize.OneVsOne, '1v1'],
    [GameSize.OneVsOneVsOne, '1v1v1'],
    [GameSize.TwoVsTwo, '2v2'],
    [GameSize.TwoVsTwoVsTwo, '2v2v2'],
];
