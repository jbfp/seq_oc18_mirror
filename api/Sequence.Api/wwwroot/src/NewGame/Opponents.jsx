import React from 'react';
import { ServerContext } from "../contexts";
import Opponent from './Opponent';

function getBackgroundColors(n) {
    switch (n) {
        case 1: return ['green'];
        case 2: return ['green', 'blue'];
        case 3: return ['green', 'red', 'green'];
        case 5: return ['green', 'blue', 'red', 'green', 'blue'];
        default: return new Array(n).fill('');
    }
}

class Opponents extends React.Component {
    static contextType = ServerContext;

    state = {
        botTypes: [],
    };

    async componentDidMount() {
        let botTypes = [];

        try {
            botTypes = await this.context.getBotsAsync();
        } catch (e) {
            console.error(e);
        }

        this.setState({ botTypes });
    }

    render() {
        const { botTypes } = this.state;
        const { opponents } = this.props;
        const { onOpponentNameChange, onOpponentTypeChange } = this.props;
        const backgroundColors = getBackgroundColors(opponents.length);

        return (
            <div>
                <div className="new-game-opponent" data-background-color="red">
                    <input
                        type="text"
                        value="You"
                        readOnly={true}
                    />
                </div>

                {opponents.map(({ name, type }, i) => (
                    <Opponent
                        key={i}
                        index={i}
                        botTypes={botTypes}
                        name={name}
                        type={type}
                        backgroundColor={backgroundColors[i]}
                        onNameChange={name => onOpponentNameChange(i, name)}
                        onTypeChange={type => onOpponentTypeChange(i, type)}
                    />
                ))}
            </div>
        )
    }
}

export default Opponents;
