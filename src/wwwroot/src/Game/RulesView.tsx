import React from 'react';
import { BoardType } from "../types";

const BoardTypeNames = new Map<BoardType, string>([
    [BoardType.OneEyedJack, 'One-Eyed Jack'],
    [BoardType.Sequence, 'Sequence®'],
]);

interface RulesViewProps {
    boardType: BoardType;
    winCondition: number;
}

export default function RulesView(props: RulesViewProps) {
    const { boardType, winCondition } = props;
    const winConditionSuffix = winCondition === 1 ? 'sequence' : 'sequences';

    return (
        <div className="rules">
            <dl>
                <dt>Board type:</dt>
                <dd>{BoardTypeNames.get(boardType)}</dd>

                <dt>Win condition:</dt>
                <dd>{winCondition} {winConditionSuffix}</dd>
            </dl>
        </div>
    );
}
