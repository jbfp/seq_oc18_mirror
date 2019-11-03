import React from 'react';
import GameListItem from './GameListItem';
import { Game } from './types';

interface GameListProps {
    games: Game[];
    userName: string;
}

export default function GameList(props: GameListProps) {
    const { games, userName } = props;

    if (games.length === 0) {
        return (
            <span className="game-list-empty">
                No games found
            </span>
        );
    }

    const $games = games.map((game) => (
        <li key={game.gameId}>
            <GameListItem {...game} userName={userName} />
        </li>
    ));

    return (
        <ul className="game-list">
            {$games}
        </ul>
    );
}
