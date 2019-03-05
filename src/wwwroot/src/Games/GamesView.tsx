import React from 'react';
import { Link } from 'react-router-dom';
import { Game, GameCollections } from './types';
import GameList from './GameList';
import './Games.css';

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
                Pick a game to play from the list, or <Link to="/new-game">click here</Link> to start a new one.
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
