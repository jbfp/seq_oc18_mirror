using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sequence.Core
{
    internal sealed class Deck
    {
        private static readonly ImmutableArray<DeckNo> _deckNos = Enum
            .GetValues(typeof(DeckNo))
            .Cast<DeckNo>()
            .ToImmutableArray();

        private static readonly ImmutableArray<Suit> _suits = Enum
            .GetValues(typeof(Suit))
            .Cast<Suit>()
            .ToImmutableArray();

        private static readonly ImmutableArray<Rank> _ranks = Enum
            .GetValues(typeof(Rank))
            .Cast<Rank>()
            .ToImmutableArray();

        private static readonly ImmutableArray<Card> _cards;

        static Deck()
        {
            var numCards = _deckNos.Length * _suits.Length * _ranks.Length;
            var builder = ImmutableArray.CreateBuilder<Card>(numCards);

            foreach (var deckNo in _deckNos)
            {
                foreach (var suit in _suits)
                {
                    foreach (var rank in _ranks)
                    {
                        builder.Add(new Card(deckNo, suit, rank));
                    }
                }
            }

            _cards = builder.ToImmutable();
        }

        private readonly Random _rng;
        private readonly List<Card> _deck;

        public Deck(Seed seed)
        {
            _rng = seed.ToRandom();
            _deck = _cards.ToList();

            // Shuffle with Fisher-Yates algorithm.
            var n = _deck.Count;
            for (int i = 0; i < n; i++)
            {
                int r = i + _rng.Next(n - i);
                var t = _deck[r];
                _deck[r] = _deck[i];
                _deck[i] = t;
            }
        }

        public int Count => _deck.Count;

        public Card Top => _deck.LastOrDefault();

        public IImmutableList<Card> DealHand()
        {
            var builder = ImmutableList.CreateBuilder<Card>();

            for (int i = 0; i < 7; i++)
            {
                var idx = _deck.Count - 1;
                var card = _deck[idx];
                builder.Add(card);
                _deck.RemoveAt(idx);
            }

            return builder.ToImmutable();
        }

        public void Remove(Card card)
        {
            if (card == null)
            {
                return;
            }

            _deck.Remove(card);
        }
    }
}
