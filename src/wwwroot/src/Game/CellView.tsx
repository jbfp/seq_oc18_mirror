import React, { useCallback, useMemo } from 'react';
import { Chip, Coord, Tile } from "../types";

interface CellViewProps {
    chip: Chip | null;
    coord: Coord;
    isHighlighted: boolean | null;
    isLatest: boolean;
    tile: Tile;
    onCoordClick: (coord: Coord) => void;
}

export function JokerView() {
    return <div className="cell" data-joker></div>;
}

export function CellView(props: CellViewProps) {
    const { coord, chip, isHighlighted, isLatest, tile } = props;
    const { onCoordClick } = props;

    const classes = useMemo(() => {
        const classes = ['cell'];

        if (isHighlighted === false) {
            classes.push('dimmed');
        }

        if (isLatest) {
            classes.push('pulse');
        }

        return classes.join(' ');
    }, [isHighlighted, isLatest]);

    const handleCoordClick = useCallback(
        () => onCoordClick(coord),
        [coord, onCoordClick]);

    const chipProps = useMemo(() => {
        return chip === null
            ? {}
            : { 'data-chip': chip.team, 'data-sequence': chip.isLocked };
    }, [chip])

    return (
        <div
            className={classes}
            onClick={handleCoordClick}
            data-suit={tile.suit}
            data-rank={tile.rank}
            {...chipProps}>
        </div>
    );
}
