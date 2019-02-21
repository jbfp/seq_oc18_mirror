import React from 'react';
import { Chip, Coord, Tile } from "../types";

interface CellViewProps {
    tile: Tile | null;
    coord: Coord;
    chip: Chip | null;
    isHighlighted: boolean | null;
    isLatest: boolean;
    onCoordClick: (coord: Coord) => void;
}

export default function CellView(props: CellViewProps) {
    const classes = ['cell'];

    if (props.tile) {
        const { coord, chip, isHighlighted, isLatest, tile } = props;
        const { onCoordClick } = props;

        if (isHighlighted === false) {
            classes.push('dimmed');
        }

        if (isLatest) {
            classes.push('pulse');
        }

        const chipProps = chip === null
            ? {}
            : { 'data-chip': chip.team, 'data-sequence': chip.isLocked };

        return (
            <div
                className={classes.join(' ')}
                onClick={() => onCoordClick(coord)}
                data-suit={tile.suit}
                data-rank={tile.rank}
                {...chipProps}>
            </div>
        );
    } else {
        return <div className={classes.join(' ')} data-joker></div>;
    }
}
