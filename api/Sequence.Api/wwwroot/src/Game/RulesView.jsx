import React from 'react';

const boardTypes = {
    'oneEyedJack': 'One-Eyed Jack',
    'sequence': 'Sequence®',
};

class RulesView extends React.PureComponent {
    render() {
        const { boardType, winCondition } = this.props;
        const winConditionSuffix = winCondition === 1 ? 'sequence' : 'sequences';

        return (
            <div className="rules">
                <dl>
                    <dt>Board type:</dt>
                    <dd>{boardTypes[boardType]}</dd>

                    <dt>Win condition:</dt>
                    <dd>{winCondition} {winConditionSuffix}</dd>
                </dl>
            </div>
        );
    }
}

export default RulesView;
