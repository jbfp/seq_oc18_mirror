import React, { useMemo } from 'react';
import * as t from '../types';

interface RulesViewProps {
    boardType: t.BoardType;
    firstPlayer: t.PlayerHandle;
    winCondition: number;
}

export default function RulesView(props: RulesViewProps) {
    const { boardType, firstPlayer, winCondition } = props;

    const boardTypeName = useMemo(() => {
        const boardTypeNames = new Map<t.BoardType, string>([
            [t.BoardType.OneEyedJack, 'One-Eyed Jack'],
            [t.BoardType.Sequence, 'Sequence®'],
        ]);

        return boardTypeNames.get(boardType);
    }, [boardType]);

    const winConditionSuffix = useMemo(() => {
        return winCondition === 1 ? 'sequence' : 'sequences';
    }, [winCondition]);

    return (
        <div className="rules">
            <dl>
                <dt>Board type:</dt>
                <dd>{boardTypeName}</dd>

                <dt>First player:</dt>
                <dd>{firstPlayer}</dd>

                <dt>Win condition:</dt>
                <dd>{winCondition} {winConditionSuffix}</dd>
            </dl>
        </div>
    );
}
