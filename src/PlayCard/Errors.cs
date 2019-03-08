namespace Sequence.PlayCard
{
    public enum PlayCardError
    {
        PlayerIsNotInGame,
        PlayerIsNotCurrentPlayer,
        CoordIsOccupied,
        PlayerDoesNotHaveCard,
        CardDoesNotMatchCoord,
        CoordIsEmpty,
        ChipBelongsToPlayerTeam,
        ChipIsPartOfSequence,
    }

    public enum ExchangeDeadCardError
    {
        PlayerIsNotInGame,
        PlayerIsNotCurrentPlayer,
        PlayerDoesNotHaveCard,
        CardIsNotDead,
        PlayerHasAlreadyExchangedDeadCard,
    }
}
