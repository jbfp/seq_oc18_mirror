import React from 'react';

class Opponent extends React.PureComponent {
    render() {
        const { botTypes, busy, index, name, type, onNameChange, onTypeChange } = this.props;

        let $input;

        if (type === 'user') {
            $input = (
                <input
                    type="text"
                    value={name}
                    onChange={event => onNameChange(event.target.value)}
                    placeholder={`Opponent #${index + 1}`}
                    readOnly={busy}
                    autoFocus={true}
                />
            );
        } else if (type === 'bot') {
            $input = (
                <select
                    value={name}
                    onChange={event => onNameChange(event.target.value)}
                    readOnly={busy}
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

        const $radio = (value, text) => (
            <label className="new-game-opponent-type">
                <input
                    type="radio"
                    value={value}
                    checked={type === value}
                    onChange={event => onTypeChange(event.target.value)}
                />
                <span>{text}</span>
            </label>
        );

        return (
            <div className="new-game-opponent">
                {$radio('user', 'User')}
                {$radio('bot', 'Bot')}
                {$input}
            </div>
        );
    }
}

export default Opponent;
