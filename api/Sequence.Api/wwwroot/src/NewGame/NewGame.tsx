import { History } from "history";
import React, { useContext, useReducer } from 'react';
import { Link } from 'react-router-dom';
import { setBoardType, setBusy, setError, setGameSize, setWinCondition } from './actions';
import { ServerContext } from '../contexts';
import { reducer } from './reducer';
import { BoardType } from "../types";
import { GameSize, NewGameState, NumSequencesToWin } from "./types";
import Opponents from "./Opponents";
import './NewGame.css';

interface NewGameProps {
    history: History;
}

export default function NewGame(props: NewGameProps) {
    const [state, dispatch] = useReducer(reducer, initialState);
    const context = useContext(ServerContext);

    async function submitAsync(event: React.FormEvent<HTMLFormElement>) {
        if (state.numSequencesToWin === null) {
            return;
        }

        event.preventDefault();
        dispatch(setBusy(true));
        dispatch(setError(null));

        let gameId;

        try {
            gameId = await context.createGameAsync({
                boardType: state.boardType,
                numSequencesToWin: state.numSequencesToWin,
                opponents: state.opponents
            });
        } catch (e) {
            dispatch(setError(e.toString()));
        } finally {
            dispatch(setBusy(false));
        }

        if (gameId) {
            props.history.push(`/games/${gameId}`);
        }
    }

    const disabled = state.opponents.length === 0
        || state.opponents.some(opponent => opponent.name.length === 0)
        || state.boardType === null
        || state.numSequencesToWin === null
        || state.busy;

    return (
        <div className="new-game">
            <p>
                Start a new game or click <Link to="/">here</Link> to go back.
            </p>

            <form onSubmit={submitAsync}>
                <div>
                    {GameSizes.map(([gameSize, name]) => (
                        <label key={gameSize} className="new-game-game-size">
                            <input
                                type="radio"
                                name="num-opponents"
                                readOnly={disabled}
                                onChange={() => dispatch(setGameSize(gameSize))}
                            />

                            <span className="new-game-game-size-text">
                                {name}
                            </span>
                        </label>
                    ))}
                </div>

                <Opponents opponents={state.opponents} dispatch={dispatch}></Opponents>

                <div className="new-game-board-types">
                    <strong>Board:</strong>

                    <label className="new-game-board-types-type">
                        <input
                            type="radio"
                            name="board-type"
                            value={BoardType.OneEyedJack}
                            checked={state.boardType === BoardType.OneEyedJack}
                            onChange={() => dispatch(setBoardType(BoardType.OneEyedJack))}
                            readOnly={disabled}
                        />

                        <span>One-Eyed Jack</span>
                    </label>

                    <label className="new-game-board-types-type">
                        <input
                            type="radio"
                            name="board-type"
                            value={BoardType.Sequence}
                            checked={state.boardType === BoardType.Sequence}
                            onChange={() => dispatch(setBoardType(BoardType.Sequence))}
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
                                value={state.numSequencesToWin || ''}
                                onFocus={e => e.target.select()}
                                onChange={event => dispatch(setWinCondition(event.target.valueAsNumber as NumSequencesToWin))}
                                readOnly={disabled}
                            />
                        </div>
                    </label>
                </div>

                <button className="new-game-submit" type="submit" disabled={disabled}>
                    Start game
                </button>

                <div>
                    <span className="new-game-error">
                        {state.error}
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

const initialState = Object.freeze<NewGameState>({
    boardType: BoardType.Sequence,
    numSequencesToWin: null,
    opponents: [],
    busy: false,
    error: null,
});
