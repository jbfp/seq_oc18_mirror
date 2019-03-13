import React, { useMemo } from 'react';
import { BoardType } from "../types";

interface RulesViewProps {
    boardType: BoardType;
    winCondition: number;
}

export default function RulesView(props: RulesViewProps) {
    const { boardType, winCondition } = props;

    const boardTypeName = useMemo(() => {
        const boardTypeNames = new Map<BoardType, string>([
            [BoardType.OneEyedJack, 'One-Eyed Jack'],
            [BoardType.Sequence, 'Sequence®'],
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

                <dt>Win condition:</dt>
                <dd>{winCondition} {winConditionSuffix}</dd>
            </dl>
        </div>
    );
}
