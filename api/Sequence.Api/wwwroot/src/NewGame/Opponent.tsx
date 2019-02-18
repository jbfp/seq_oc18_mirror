import React from 'react';
import { BackgroundColor, BotType, OpponentType } from './types';

interface OpponentProps {
    backgroundColor: BackgroundColor,
    botTypes: BotType[];
    index: number;
    name: string;
    type: OpponentType;
    onNameChange: (name: string) => void;
    onTypeChange: (type: OpponentType) => void;
}

const OpponentTypes: Array<[OpponentType, string]> = [
    [OpponentType.User, 'User'],
    [OpponentType.Bot, 'Bot']
];

class Opponent extends React.PureComponent<OpponentProps> {
    render() {
        const { backgroundColor, botTypes, index, name, type } = this.props;
        const { onNameChange, onTypeChange } = this.props;

        let $input;

        if (type === OpponentType.User) {
            $input = (
                <input
                    type="text"
                    value={name}
                    onChange={event => onNameChange(event.target.value)}
                    placeholder={`Opponent #${index + 1}`}
                    autoFocus={true}
                />
            );
        } else if (type === OpponentType.Bot) {
            $input = (
                <select
                    value={name}
                    onChange={event => onNameChange(event.target.value)}
                    autoFocus={true}
                >
                    <option value="">Select bot type</option>
                    {botTypes.map(botType => (
                        <option key={botType} value={botType}>{botType}</option>
                    ))}
                </select>
            );
        } else {
            throw new Error(`'Type '${type}' is invalid.`);
        }

        return (
            <div className="new-game-opponent" data-background-color={backgroundColor}>
                {OpponentTypes.map(([value, text]) => (
                    <label key={value} className="new-game-opponent-type">
                        <input
                            type="radio"
                            checked={type === value}
                            onChange={() => onTypeChange(value)}
                        />

                        <span>{text}</span>
                    </label>
                ))}

                {$input}
            </div>
        );
    }
}

export default Opponent;
