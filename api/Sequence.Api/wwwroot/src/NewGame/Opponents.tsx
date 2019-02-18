import React from 'react';
import { NewGameAction, setOpponentName, setOpponentType } from "./actions";
import { ServerContext } from "../contexts";
import { BackgroundColor, BotType, OpponentType } from './types';
import Opponent from './Opponent';

interface OpponentsProps {
    opponents: Array<{ name: string, type: OpponentType }>;
    dispatch: (action: NewGameAction) => void;
}

interface OpponentsState {
    botTypes: BotType[];
}

class Opponents extends React.Component<OpponentsProps, OpponentsState> {
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
        const { dispatch } = this.props;
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
                        onNameChange={name => dispatch(setOpponentName(i, name))}
                        onTypeChange={type => dispatch(setOpponentType(i, type))}
                    />
                ))}
            </div>
        )
    }
}

export default Opponents;

function getBackgroundColors(n: number): BackgroundColor[] {
    switch (n) {
        case 1: return [BackgroundColor.Green];
        case 2: return [BackgroundColor.Green, BackgroundColor.Blue];
        case 3: return [BackgroundColor.Green, BackgroundColor.Red, BackgroundColor.Green];
        case 5: return [BackgroundColor.Green, BackgroundColor.Blue, BackgroundColor.Red, BackgroundColor.Green, BackgroundColor.Blue];
        default: return new Array(n).fill(BackgroundColor.None);
    }
}
