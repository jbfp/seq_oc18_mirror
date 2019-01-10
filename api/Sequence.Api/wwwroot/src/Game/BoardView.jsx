import PropTypes from 'prop-types';
import React from 'react';
import CellView from './CellView';

class BoardView extends React.PureComponent {
    static propTypes = {
        board: PropTypes.arrayOf(
            PropTypes.arrayOf(PropTypes.object).isRequired,
        ).isRequired,

        chips: PropTypes.arrayOf(PropTypes.shape({
            coord: PropTypes.shape({
                column: PropTypes.number.isRequired,
                row: PropTypes.number.isRequired,
            }),

            isLocked: PropTypes.bool.isRequired,
            team: PropTypes.oneOf(['red', 'green', 'blue']).isRequired,
        })).isRequired,

        latestMoveAt: PropTypes.object,

        onCoordClick: PropTypes.func.isRequired,
    };

    render() {
        const { board, chips, highlightedCellValue, latestMoveAt, onCoordClick } = this.props;

        const numRows = board.length;
        const numCols = Math.max.apply(Math, board.map(r => r.length));

        const style = {
            'gridTemplateColumns': `repeat(${numCols}, 67px)`,
            'gridTemplateRows': `repeat(${numRows}, 50px)`,
        };

        // TODO: Memoize cells.
        const cells = board.map((cells, row) => {
            return cells.map((cell, column) => {
                const chip = chips.find(chip => chip.coord.row === row && chip.coord.column === column) || { team: null, isLocked: false };
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
                        chip.team === null;

                    // TODO: Make sure we don't show the player's own teams' chips.
                    const isOneEyedJack =
                        highlightedCellValue.rank === 'jack' &&
                        (highlightedCellValue.suit === 'hearts' || highlightedCellValue.suit === 'spades') &&
                        chip.team;

                    isHighlighted = matchesTile || isTwoEyedJack || isOneEyedJack;
                }

                return { key: `${column}_${row}`, tile, row, column, chip, onCoordClick, isHighlighted, isLatest };
            });
        }).flat().map(cell => <CellView {...cell} />);

        return (
            <div className="board" style={style}>
                {cells}
            </div>
        )
    }
}

export default BoardView;
