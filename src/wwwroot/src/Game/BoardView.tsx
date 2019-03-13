import React, { useMemo } from 'react';
import * as t from '../types';
import { coordKey } from './helpers';
import { CellView, JokerView } from './CellView';

interface BoardViewProps {
    board: t.Board;
    chips: Map<string, t.Chip>;
    highlightedCellValue: t.Tile | null;
    latestMoveAt: t.Coord | null;
    onCoordClick: (coord: t.Coord) => void;
}

export default function BoardView(props: BoardViewProps) {
    const { board, chips, highlightedCellValue, latestMoveAt } = props;
    const { onCoordClick } = props;

    const latestMoveAtKey = useMemo(() => {
        if (latestMoveAt) {
            return coordKey(latestMoveAt);
        }

        return null;
    }, [latestMoveAt]);

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
                const key = coordKey(coord);
                return { cell, coord, key };
            });
        }).flat();
    }, [board]);

    const cells = flat.map(({ cell, coord, key }) => {
        if (cell) {
            const chip = chips.get(key) || null;
            const isLatest = key === latestMoveAtKey;

            let isHighlighted = null;

            if (highlightedCellValue) {
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
        } else {
            return <JokerView key={key} />
        }
    });

    return (
        <div className="board" style={style}>
            {cells}
        </div>
    );
}
