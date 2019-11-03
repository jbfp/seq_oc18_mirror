import * as t from '../types';

export function keyedArray<TKey, TValue>(
    { xs, mapKey }: { xs: TValue[]; mapKey: (x: TValue) => TKey; }): ReadonlyArray<[TKey, TValue]> {
    return xs.map<[TKey, TValue]>((x) => [mapKey(x), x]);
}

export function cardKey(card: t.Card): string {
    return `${card.deckNo}_${card.suit}_${card.rank}`;
}

export function coordKey(coord: t.Coord): string {
    return `${coord.column}_${coord.row}`;
}
