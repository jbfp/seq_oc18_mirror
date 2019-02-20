import React from 'react';
import { Chip, Coord, Tile } from "../types";

interface CellViewProps {
    tile: Tile;
    row: number;
    column: number;
    chip: Chip;
    isHighlighted: boolean;
    isLatest: boolean;
    onCoordClick: (coord: Coord) => void;
}

export default function CellView(props: CellViewProps) {
    const { tile, row, column, chip, isHighlighted, isLatest, onCoordClick } = props;
    const classes = ['cell'];

    if (tile) {
        if (isHighlighted === false) {
            classes.push('dimmed');
        }

        if (isLatest) {
            classes.push('pulse');
        }

        return (
            <div
                className={classes.join(' ')}
                data-suit={tile.suit}
                data-rank={tile.rank}
                data-chip={chip.team}
                data-sequence={chip.isLocked}
                onClick={() => onCoordClick({ column, row })}>
            </div>
        );
    } else {
        return <div className={classes.join(' ')} data-joker></div>;
    }
}
