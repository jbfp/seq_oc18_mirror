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
        private readonly int _numPlayers;

        public Deck(Seed seed, int numPlayers)
        {
            _rng = seed.ToRandom();
            _deck = _cards.ToList();
            _numPlayers = numPlayers;

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

        public IImmutableList<IImmutableList<Card>> DealHands()
        {
            int GetNumCards()
            {
                switch (_numPlayers)
                {
                    case 2: return 7;
                    case 3: return 6;
                    case 4:
                    case 6: return 5;
                    default: throw new NotSupportedException();
                }
            }

            IImmutableList<Card> DealHand(int n)
            {
                var builder = ImmutableList.CreateBuilder<Card>();

                for (int i = 0; i < n; i++)
                {
                    var idx = _deck.Count - 1;
                    var card = _deck[idx];
                    builder.Add(card);
                    _deck.RemoveAt(idx);
                }

                return builder.ToImmutable();
            }

            int numCards = GetNumCards();

            return Enumerable
                .Range(0, _numPlayers)
                .Select(_ => DealHand(numCards))
                .ToImmutableList();
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
