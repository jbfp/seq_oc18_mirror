import React, { useMemo } from 'react';
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

    const style = useMemo(() => {
        const numRows = board.length;
        const numCols = Math.max.apply(Math, board.map(r => r.length));

        return {
            'gridTemplateColumns': `repeat(${numCols}, ${100 / numCols}%)`,
            'gridTemplateRows': `repeat(${numRows}, ${100 / numRows}%)`,
        };
    }, [board]);

    const flat = useMemo(() => {
        return board.map((cells, row) => {
            return cells.map((cell, column) => {
                const coord: t.Coord = { column, row };
                const key = `${column}_${row}`;
                return { cell, coord, key };
            });
        }).flat();
    }, [board]);

    const cells = flat.map(({ cell, coord, key }) => {
        const chip = chips.find(chip =>
            chip.coord.column === coord.column &&
            chip.coord.row === coord.row
        ) || null;

        const isLatest =
            latestMoveAt !== null &&
            latestMoveAt.column == coord.column &&
            latestMoveAt.row === coord.row;

        let isHighlighted = null;

        if (cell && highlightedCellValue) {
            const matchesCellValue =
                cell.suit === highlightedCellValue.suit &&
                cell.rank === highlightedCellValue.rank;

            const isTwoEyedJack =
                highlightedCellValue.rank === 'jack' &&
                (highlightedCellValue.suit === 'diamonds' || highlightedCellValue.suit === 'clubs') &&
                (chip === null || chip.team === null);

            // TODO: Make sure we don't show the player's own teams' chips.
            const isOneEyedJack =
                highlightedCellValue.rank === 'jack' &&
                (highlightedCellValue.suit === 'hearts' || highlightedCellValue.suit === 'spades') &&
                chip !== null && chip.team !== null;

            isHighlighted = matchesCellValue || isTwoEyedJack || isOneEyedJack;
        }

        return (
            <CellView
                key={key}
                chip={chip}
                coord={coord}
                isHighlighted={isHighlighted}
                isLatest={isLatest}
                tile={cell}
                onCoordClick={onCoordClick} />
        );
    });

    return (
        <div className="board" style={style}>
            {cells}
        </div>
    );
}
