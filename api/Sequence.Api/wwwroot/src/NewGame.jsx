import React from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from "./contexts";
import './NewGame.css';

class NewGame extends React.Component {
    static contextType = ServerContext;

    state = {
        botTypes: [],
        opponents: [],
        busy: false,
        error: null,
    };

    handleSubmit = async event => {
        event.preventDefault();

        let gameId;

        this.setState({ busy: true, error: null });

        try {
            gameId = await this.context.createGameAsync(this.state.opponents);
            this.setState({ busy: false, opponents: [] });
            this.props.history.push(`/games/${gameId}`);
        } catch (e) {
            this.setState({ busy: false, error: e.toString() });
        }
    };

    handleOpponentChange = (i, event) => {
        const opponents = this.state.opponents.map((o, j) => {
            if (i === j) {
                return event.target.value;
            } else {
                return o;
            }
        });

        this.setState({ opponents });
    };

    setGameSize = numOpponents => {
        const opponents = new Array(numOpponents);
        opponents.fill('');
        this.setState({ opponents })
    }

    async componentDidMount() {
        this.setState({ botTypes: await this.context.getBotsAsync() });
    }

    render() {
        const { botTypes, opponents, busy, error } = this.state;
        const disabled = opponents.length === 0
            || opponents.includes('')
            || busy;

        const $opponents = opponents.map((_, i) => (
            <div key={i} className="new-game-opponent">
                <input
                    type="text"
                    value={opponents[i]}
                    onChange={e => this.handleOpponentChange(i, e)}
                    placeholder={`Opponent #${i + 1}`}
                    readOnly={busy}
                />
            </div>
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
