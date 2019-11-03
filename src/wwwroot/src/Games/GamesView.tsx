import React from 'react';
import { Link } from 'react-router-dom';
import GameList from './GameList';
import './Games.css';
import { Game, GameCollections } from './types';

interface GamesViewProps {
    games: GameCollections;
    userName: string;
}

export default function GamesView(props: GamesViewProps) {
    const { games, userName } = props;

    interface GameListWithUserNameProps {
        games: Game[];
    }

    const GameListWithUserName = (props: GameListWithUserNameProps) => (
        <GameList games={props.games} userName={userName} />
    );

    return (
        <div className="games">
            <p>
                <Link to="/new-game">New Game</Link>
                &nbsp;&nbsp;
                <Link to="/new-simulation">New Simulation</Link>
            </p>

            <div>
                <h3>Your turn</h3>

                <div>
                    <GameListWithUserName games={games.yourTurn} />
                </div>
            </div>

            <div>
                <h3>Their turn</h3>

                <div>
                    <GameListWithUserName games={games.theirTurn} />
                </div>
            </div>

            <div>
                <h3>Completed games</h3>

                <div>
                    <GameListWithUserName games={games.completedGames} />
                </div>
            </div>
        </div>
    );
}
