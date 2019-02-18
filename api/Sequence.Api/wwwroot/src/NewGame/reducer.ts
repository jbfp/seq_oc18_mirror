import { NewGameAction, NewGameActionKind } from './actions';
import { GameSize, NewGameState, NumSequencesToWin, Opponent, OpponentType } from './types';

export function reducer(state: Readonly<NewGameState>, action: NewGameAction): Readonly<NewGameState> {
    switch (action.kind) {
        case NewGameActionKind.SetBoardType: {
            return Object.freeze({ ...state, boardType: action.newBoardType });
        }

        case NewGameActionKind.SetBusy: {
            return Object.freeze({ ...state, busy: action.newBusy });
        }

        case NewGameActionKind.SetError: {
            return Object.freeze({ ...state, error: action.newError });
        }

        case NewGameActionKind.SetGameSize: {
            const numOpponents = action.newGameSize;
            const opponents = new Array<Opponent>(numOpponents).fill({ ...defaultOpponent });
            const numSequencesToWin = getDefaultNumSequences(numOpponents);
            return Object.freeze({ ...state, numSequencesToWin, opponents });
        }

        case NewGameActionKind.SetOpponentName:
        case NewGameActionKind.SetOpponentType: {
            const partial: Partial<Opponent> = action.kind === NewGameActionKind.SetOpponentName
                ? { name: action.newOpponentName }
                : { type: action.newOpponentType };

            const i = action.index;
            const oldOpponents = state.opponents;
            const oldOpponent = oldOpponents[i];
            const newOpponent = update(oldOpponent, partial);
            const newOpponents = replace(oldOpponents, newOpponent, i);
            return Object.freeze({ ...state, opponents: newOpponents });
        }

        case NewGameActionKind.SetWinCondition: {
            return Object.freeze({ ...state, numSequencesToWin: action.newWinCondition });
        }
    }
}

const defaultOpponent = Object.freeze<Opponent>({
    name: '',
    type: OpponentType.User
});

function getDefaultNumSequences(gameSize: GameSize): NumSequencesToWin {
    switch (gameSize) {
        case GameSize.OneVsOne:
        case GameSize.OneVsOneVsOne: return 2;
        case GameSize.TwoVsTwo:
        case GameSize.TwoVsTwoVsTwo: return 1;
    }
}

function update<T>(opponent: T, update: Partial<T>): T {
    return { ...opponent, ...update };
}

function replace<T>(opponents: T[], opponent: T, i: number): T[] {
    const begin = opponents.slice(0, i);
    const after = opponents.slice(i + 1);
    return [...begin, opponent, ...after];
}
