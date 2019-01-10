import React from 'react';

class CellView extends React.PureComponent {
    render() {
        const { tile, row, column, chip, isHighlighted, isLatest, onCoordClick } = this.props;
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
}

export default CellView;
