import React from 'react';
import { Link } from 'react-router-dom';
import Opponent from './Opponent';
import { ServerContext } from "../contexts";
import './NewGame.css';

const defaultOpponent = { name: '', type: 'user' };

function getDefaultNumSequencesToWin(numOpponents) {
    switch (numOpponents) {
        case 1:
        case 2: return 2;
        case 3:
        case 5: return 1;
        default: throw new Error(`${numOpponents} is not valid.`);
    }
}

// Number.isSafeInteger polyfill:
Number.isSafeInteger = Number.isSafeInteger || function (value) {
    return Number.isInteger(value) && Math.abs(value) <= Number.MAX_SAFE_INTEGER;
};

class NewGame extends React.Component {
    static contextType = ServerContext;

    state = {
        boardType: 0,
        botTypes: [],
        numSequencesToWin: null,
        opponents: [],
        busy: false,
        error: null,
    };

    handleOpponentNameChange = (i, name) => {
        this.setOpponent(i, { name });
    };

    handleOpponentTypeChange = (i, type) => {
        this.setOpponent(i, { type, name: '' });
    };

    setOpponent(i, update) {
        const begin = this.state.opponents.slice(0, i);
        const opponent = this.state.opponents[i];
        const after = this.state.opponents.slice(i + 1);
        const opponents = [...begin, { ...opponent, ...update }, ...after];
        this.setState({ opponents });
    }

    handleNumSequencesToWinChange = event => {
        if (event.target.checkValidity()) {
            event.preventDefault();
            const value = event.target.value;
            const numSequencesToWin = Number.parseInt(value, 10);

            if (Number.isSafeInteger(numSequencesToWin)) {
                this.setState({ numSequencesToWin });
            }
        }
    }

    handleSubmit = async event => {
        event.preventDefault();

        let gameId;

        this.setState({ busy: true, error: null });

        const { opponents, boardType, numSequencesToWin } = this.state;

        try {
            gameId = await this.context.createGameAsync(opponents, boardType, numSequencesToWin);
            console.log(numSequencesToWin);
            this.setState({ busy: false, opponents: [] });
            this.props.history.push(`/games/${gameId}`);
        } catch (e) {
            this.setState({ busy: false, error: e.toString() });
        }
    };

    setGameSize = numOpponents => {
        const opponents = new Array(numOpponents);
        opponents.fill({ ...defaultOpponent });
        const numSequencesToWin = getDefaultNumSequencesToWin(numOpponents);
        this.setState({ opponents, numSequencesToWin });
    }

    setBoardType = boardType => {
        this.setState({ boardType });
    };

    async componentDidMount() {
        this.setState({ botTypes: await this.context.getBotsAsync() });
    }

    render() {
        const { boardType, botTypes, numSequencesToWin, opponents, busy, error } = this.state;
        const disabled = opponents.length === 0
            || opponents.some(opponent => !opponent.name)
            || boardType === null
            || numSequencesToWin === null
            || busy;

        const $opponents = opponents.map(({ name, type }, i) => (
            <Opponent key={i}
                index={i}
                botTypes={botTypes}
                name={name}
                type={type}
                onNameChange={name => this.handleOpponentNameChange(i, name)}
                onTypeChange={type => this.handleOpponentTypeChange(i, type)}
            />
        ));

        return (
            <div className="new-game">
                <p>
                    Start a new game or click <Link to="/">here</Link> to go back.
                </p>

                <form onSubmit={this.handleSubmit}>
                    <div>
                        <label className="new-game-game-size">
                            <input
                                type="radio"
                                name="num-opponents"
                                onChange={() => this.setGameSize(1)}
                            />

                            <span className="new-game-game-size-text">1v1</span>
                        </label>

                        <label className="new-game-game-size">
                            <input
                                type="radio"
                                name="num-opponents"
                                onChange={() => this.setGameSize(2)}
                            />

                            <span className="new-game-game-size-text">1v1v1</span>
                        </label>

                        <label className="new-game-game-size">
                            <input
                                type="radio"
                                name="num-opponents"
                                onChange={() => this.setGameSize(3)}
                            />

                            <span className="new-game-game-size-text">2v2</span>
                        </label>

                        <label className="new-game-game-size">
                            <input
                                type="radio"
                                name="num-opponents"
                                onChange={() => this.setGameSize(5)}
                            />

                            <span className="new-game-game-size-text">2v2v2</span>
                        </label>
                    </div>

                    <div>
                        <div className="new-game-opponent">
                            <input
                                type="text"
                                value="You"
                                readOnly={true}
                            />
                        </div>

                        {$opponents}
                    </div>

                    <div className="new-game-board-types">
                        <strong>Board:</strong>

                        <label className="new-game-board-types-type">
                            <input
                                type="radio"
                                name="board-type"
                                value={0}
                                checked={boardType === 0}
                                onChange={() => this.setBoardType(0)}
                                readOnly={busy}
                            />

                            <span>One-Eyed Jack</span>
                        </label>

                        <label className="new-game-board-types-type">
                            <input
                                type="radio"
                                name="board-type"
                                value={1}
                                checked={boardType === 1}
                                onChange={() => this.setBoardType(1)}
                                readOnly={busy}
                            />

                            <span>Sequence®</span>
                        </label>
                    </div>

                    {numSequencesToWin === null ? null : (
                        <div className="new-game-win-condition">
                            <label className="new-game-win-condition-container">
                                <strong className="new-game-win-condition-container-text">Win condition:</strong>

                                <div className="new-game-win-condition-input-container">
                                    <input
                                        className="new-game-win-condition-input"
                                        type="number"
                                        min={1}
                                        max={4}
                                        value={numSequencesToWin}
                                        onChange={this.handleNumSequencesToWinChange}
                                        readOnly={busy}
                                    /> sequences to win
                                </div>
                            </label>
                        </div>
                    )}

                    <button className="new-game-submit" type="submit" disabled={disabled}>
                        Start game
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
}

export default NewGame;
