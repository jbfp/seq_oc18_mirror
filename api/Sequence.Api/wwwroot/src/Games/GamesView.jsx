import React from 'react';
import { Link } from 'react-router-dom';
import GameList from "./GameList";
import './Games.css';

class GamesView extends React.PureComponent {
    render() {
        const { games, userName } = this.props;

        if (games) {
            return (
                <div className="games">
                    <p>
                        Pick a game to play from the list, or <Link to="/new-game">click here</Link> to start a new one.
                    </p>

                    <div>
                        <h3>Your turn</h3>

                        <div>
                            <GameList games={games.yourTurn} userName={userName} />
                        </div>
                    </div>

                    <div>
                        <h3>Their turn</h3>

                        <div>
                            <GameList games={games.theirTurn} userName={userName} />
                        </div>
                    </div>

                    <div>
                        <h3>Completed games</h3>

                        <div>
                            <GameList games={games.completedGames} userName={userName} />
                        </div>
                    </div>
                </div>
            );
        } else {
            return (
                <div>
                    <p>Loading games...</p>
                </div>
            );
        }
    }
}

export default GamesView;
