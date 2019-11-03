import React from 'react';
import { ServerContext } from '../contexts';
import { NewGameAction, setOpponentName, setOpponentType } from './actions';
import Opponent from './Opponent';
import { BackgroundColor, BotType, OpponentType } from './types';

interface OpponentsProps {
    opponents: Array<{ name: string, type: OpponentType }>;
    dispatch: (action: NewGameAction) => void;
}

interface OpponentsState {
    botTypes: BotType[];
}

class Opponents extends React.Component<OpponentsProps, OpponentsState> {
    public static contextType = ServerContext;

    public state = {
        botTypes: [],
    };

    public async componentDidMount() {
        let botTypes = [];

        try {
            botTypes = await this.context.getBotsAsync();
        } catch (e) {
            console.error(e);
        }

        this.setState({ botTypes });
    }

    public render() {
        const { botTypes } = this.state;
        const { opponents } = this.props;
        const { dispatch } = this.props;
        const backgroundColors = getBackgroundColors(opponents.length);
        const $opponents = opponents.map(({ name, type }, i) => {
            const handleNameChange = (name: string) => dispatch(setOpponentName(i, name));
            const handleTypeChange = (type: OpponentType) => dispatch(setOpponentType(i, type));

            return (
                <Opponent
                    key={i}
                    index={i}
                    botTypes={botTypes}
                    name={name}
                    type={type}
                    backgroundColor={backgroundColors[i]}
                    onNameChange={handleNameChange}
                    onTypeChange={handleTypeChange}
                />
            );
        });

        return (
            <div>
                <div className="new-game-opponent" data-background-color="red">
                    <input
                        type="text"
                        value="You"
                        readOnly={true}
                    />
                </div>

                {$opponents}
            </div >
        );
    }
}

export default Opponents;

function getBackgroundColors(n: number): BackgroundColor[] {
    switch (n) {
        case 1: return [BackgroundColor.Green];
        case 2: return [BackgroundColor.Green, BackgroundColor.Blue];
        case 3: return [BackgroundColor.Green, BackgroundColor.Red, BackgroundColor.Green];
        case 5: return [BackgroundColor.Green, BackgroundColor.Blue, BackgroundColor.Red,
        BackgroundColor.Green, BackgroundColor.Blue];
        default: return new Array(n).fill(BackgroundColor.None);
    }
}
