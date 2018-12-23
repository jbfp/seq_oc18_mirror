import React from 'react';

class CellView extends React.PureComponent {
    render() {
        const { tile, row, column, chip, isHighlighted, onCoordClick } = this.props;
        const classes = ['cell'];

        if (tile) {
            if (isHighlighted === false) {
                classes.push('dimmed');
            }

            return (
                <div
                    className={classes.join(' ')}
                    data-suit={tile[0]}
                    data-rank={tile[1]}
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
