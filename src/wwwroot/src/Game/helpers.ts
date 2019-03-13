import * as t from '../types';

export function keyedArray<TKey, TValue>(
    xs: TValue[],
    map: (x: TValue) => TKey
): ReadonlyArray<[TKey, TValue]> {
    return xs.map<[TKey, TValue]>(x => [map(x), x]);
}

export function cardKey(card: t.Card): string {
    return `${card.deckNo}_${card.suit}_${card.rank}`;
}

export function coordKey(coord: t.Coord): string {
    return `${coord.column}_${coord.row}`;
}
