import PropTypes from 'prop-types';
import React from 'react';
import CellView from './CellView';

class BoardView extends React.PureComponent {
    static propTypes = {
        board: PropTypes.arrayOf(
            PropTypes.arrayOf(PropTypes.array).isRequired,
        ).isRequired,

        chips: PropTypes.arrayOf(PropTypes.shape({
            coord: PropTypes.shape({
                column: PropTypes.number.isRequired,
                row: PropTypes.number.isRequired,
            }),

            isLocked: PropTypes.bool.isRequired,
            team: PropTypes.oneOf(['red', 'green', 'blue']).isRequired,
        })).isRequired,

        onCoordClick: PropTypes.func.isRequired,
    };

    render() {
        const { board, chips, highlightedCellValue, onCoordClick } = this.props;

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

                let isHighlighted = null;

                if (tile && highlightedCellValue) {
                    const matchesTile =
                        tile[0] === highlightedCellValue.suit &&
                        tile[1] === highlightedCellValue.rank;

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

                return { tile, row, column, chip, onCoordClick, isHighlighted };
            });
        }).flat().map((cell, idx) => <CellView key={idx} {...cell} />);

        return (
            <div className="board" style={style}>
                {cells}
            </div>
        )
    }
}

export default BoardView;
