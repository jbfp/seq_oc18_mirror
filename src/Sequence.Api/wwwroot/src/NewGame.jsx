import React from 'react';
import { Link } from 'react-router-dom';
import { ServerContext } from "./contexts";
import './NewGame.css';

class NewGame extends React.Component {
    static contextType = ServerContext;

    state = {
        opponent: '',
        busy: false,
        error: null,
    };

    handleSubmit = async event => {
        event.preventDefault();

        let gameId;

        this.setState({ busy: true, error: null });

        try {
            gameId = await this.context.createGameAsync(this.state.opponent);
            this.setState({ busy: false, opponent: '' });
            this.props.history.push(`/games/${gameId}`);
        } catch (e) {
            this.setState({ busy: false, error: e.toString() });
        }
    };

    handleOpponentChange = event => {
        event.preventDefault();
        this.setState({ opponent: event.target.value });
    }

    render() {
        const { opponent, busy, error } = this.state;
        const disabled = opponent.length === 0 || busy;

        return (
            <div className="new-game">
                <p>
                    Start a new game or click <Link to="/">here</Link> to go back.
                </p>

                <form onSubmit={this.handleSubmit}>
                    <input
                        type="text"
                        value={opponent}
                        onChange={this.handleOpponentChange}
                        placeholder="Opponent name"
                        readOnly={busy}
                        autoFocus={true}
                    />

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
