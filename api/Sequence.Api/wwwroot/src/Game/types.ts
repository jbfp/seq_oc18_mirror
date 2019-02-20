import { Card, Chip, Coord, PlayerId, Sequence, Team } from '../types';

export interface GameEvent {
    byPlayerId: PlayerId;
    cardDrawn: Card | null | boolean;
    cardUsed: Card;
    chip: Chip;
    coord: Coord;
    index: number;
    nextPlayerId: PlayerId | null;
    sequence: Sequence | null;
    winner: Team | null;
}
