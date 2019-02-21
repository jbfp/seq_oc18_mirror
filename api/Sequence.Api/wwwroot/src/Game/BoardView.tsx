import React from 'react';
import * as t from '../types';
import CellView from './CellView';

interface BoardViewProps {
    board: t.Board;
    chips: t.Chip[];
    highlightedCellValue: t.Tile | null;
    latestMoveAt: t.Coord | null;
    onCoordClick: (coord: t.Coord) => void;
}

export default function BoardView(props: BoardViewProps) {
    const { board, chips, highlightedCellValue, latestMoveAt } = props;
    const { onCoordClick } = props;

    const numRows = board.length;
    const numCols = Math.max.apply(Math, board.map(r => r.length));

    const style = {
        'gridTemplateColumns': `repeat(${numCols}, ${100 / numCols}%)`,
        'gridTemplateRows': `repeat(${numRows}, ${100 / numRows}%)`,
    };

    // TODO: Memoize cells.
    const cells = board.map((cells, row) => {
        return cells.map((cell, column) => {
            const coord = { row, column };
            const chip = chips.find(chip => chip.coord.row === row && chip.coord.column === column) || null;
            const tile = cell === null ? null : { ...cell };
            const isLatest = latestMoveAt !== null && column === latestMoveAt.column && row === latestMoveAt.row;

            let isHighlighted = null;

            if (tile && highlightedCellValue) {
                const matchesTile =
                    tile.suit === highlightedCellValue.suit &&
                    tile.rank === highlightedCellValue.rank;

                const isTwoEyedJack =
                    highlightedCellValue.rank === 'jack' &&
                    (highlightedCellValue.suit === 'diamonds' || highlightedCellValue.suit === 'clubs') &&
                    (chip === null || chip.team === null);

                // TODO: Make sure we don't show the player's own teams' chips.
                const isOneEyedJack =
                    highlightedCellValue.rank === 'jack' &&
                    (highlightedCellValue.suit === 'hearts' || highlightedCellValue.suit === 'spades') &&
                    chip !== null && chip.team !== null;

                isHighlighted = matchesTile || isTwoEyedJack || isOneEyedJack;
            }

            return { key: `${column}_${row}`, tile, coord, chip, onCoordClick, isHighlighted, isLatest };
        });
    }).flat().map(cell => <CellView {...cell} />);

    return (
        <div className="board" style={style}>
            {cells}
        </div>
    );
}
