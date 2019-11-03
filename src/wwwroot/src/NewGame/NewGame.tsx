import { History } from 'history';
import shuffle from 'lodash.shuffle';
import React, { useCallback, useContext, useReducer } from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from '../contexts';
import { BoardType } from '../types';
import { setBoardType, setBusy, setError, setGameSize, setOpponents, setRandomFirstPlayer, setWinCondition } from './actions';
import './NewGame.css';
import Opponents from './Opponents';
import { reducer } from './reducer';
import { GameSize, NewGameState, NumSequencesToWin, Opponent, OpponentType } from './types';

interface NewGameProps {
    history: History;
}

export default function NewGame(props: NewGameProps) {
    const query = new URLSearchParams(props.history.location.search);

    let init = { ...initialState };

    try {
        if (query.has('board-type')) {
            init.boardType = query.getAll('board-type')[0] as BoardType;
        }

        if (query.has('win-condition')) {
            init.numSequencesToWin = Number.parseInt(query.getAll('win-condition')[0], 10) as NumSequencesToWin;
        }

        if (query.has('num-players')) {
            let numPlayers = Number.parseInt(query.getAll('num-players')[0], 10) - 1;

            if (numPlayers > GameSize.TwoVsTwoVsTwo) {
                numPlayers = GameSize.TwoVsTwoVsTwo;
            }

            while (typeof GameSize[numPlayers] === 'undefined') {
                numPlayers++;
            }

            const opponentNames = query.getAll('opponent-name');
            const opponentTypes = query.getAll('opponent-type');

            init.opponents = new Array<Opponent>(numPlayers);

            for (let i = 0; i < numPlayers; i++) {
                const name = opponentNames[i] || '';
                const typeStr = opponentTypes[i] || '';
                let type: OpponentType;

                if (typeStr === 'user') {
                    type = OpponentType.User;
                } else if (typeStr === 'bot') {
                    type = OpponentType.Bot;
                } else {
                    type = OpponentType.User;
                }

                init.opponents[i] = { name, type };
            }
        }
    } catch (err) {
        console.error('Failed to init game from query', err);
        init = initialState;
    }

    const [state, dispatch] = useReducer(reducer, init);
    const context = useContext(ServerContext);

    function randomizeOpponentOrder() {
        const opponents = shuffle(state.opponents);
        const action = setOpponents(opponents);
        dispatch(action);
    }

    const handleRandomFirstPlayerChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        dispatch(setRandomFirstPlayer(event.target.checked));
    }, []);

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
                opponents: state.opponents,
                randomFirstPlayer: state.randomFirstPlayer,
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
        || state.opponents.some((opponent) => opponent.name.length === 0)
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

                <Opponents opponents={state.opponents} dispatch={dispatch} />

                <label>
                    <input
                        type="checkbox"
                        checked={state.randomFirstPlayer}
                        onChange={handleRandomFirstPlayerChange}
                    /> Random first player
                </label>

                <button className="new-game-randomize-order-btn" onClick={randomizeOpponentOrder} type="button">
                    Randomize player order
                </button>

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
                                onFocus={(e) => e.target.select()}
                                onChange={(event) => dispatch(setWinCondition(event.target.valueAsNumber as NumSequencesToWin))}
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
    busy: false,
    error: null,
    numSequencesToWin: null,
    opponents: [],
    randomFirstPlayer: true,
});
