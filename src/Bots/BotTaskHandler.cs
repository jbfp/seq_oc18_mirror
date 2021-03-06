using Microsoft.Extensions.Logging;
using Sequence.PlayCard;
using Sequence.RealTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Bots
{
    public sealed class BotTaskHandler
    {
        private readonly PlayCardHandler _playCardHandler;
        private readonly ILogger _logger;

        public BotTaskHandler(
            PlayCardHandler playCardHandler,
            ILogger<BotTaskHandler> logger)
        {
            _playCardHandler = playCardHandler ?? throw new ArgumentNullException(nameof(playCardHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleBotTaskAsync(BotTask botTask, CancellationToken cancellationToken)
        {
            if (botTask == null)
            {
                throw new ArgumentNullException(nameof(botTask));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var gameId = botTask.GameId;
            var playerId = botTask.Player.Id;
            IImmutableList<Move> moves;

            try
            {
                moves = await _playCardHandler.GetMovesForPlayerAsync(
                    gameId, playerId, cancellationToken);
            }
            catch (GameNotFoundException)
            {
                return;
            }

            var playerHandle = botTask.Player.Handle;
            var botTypeKey = playerHandle.ToString();

            if (BotProvider.BotTypes.TryGetValue(botTypeKey, out var botType))
            {
                var bot = (IBot)Activator.CreateInstance(botType, nonPublic: true);

                IEnumerable<GameUpdated> gameEvents = null;

                // Make it look like the bot is thinking... :)
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                for (int attempt = 1; attempt <= 10; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Move move = null;

                    try
                    {
                        move = bot.Decide(moves);
                    }
                    catch (NotImplementedException)
                    {
                        continue;
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "#{Attempt}: Bot {Bot} threw an unhandled exception in {GameId}",
                            attempt, botType.Name, gameId);

                        continue;
                    }

                    if (move == null)
                    {
                        _logger.LogWarning(
                            "#{Attempt}: Bot {Bot} could not produce a move in {GameId}",
                            attempt, botType.Name, gameId);

                        break;
                    }

                    var (card, coord) = move;

                    if (coord.Equals(new Coord(-1, -1)))
                    {
                        try
                        {
                            gameEvents = await _playCardHandler.ExchangeDeadCardAsync(gameId,
                                playerId, card, cancellationToken);
                        }
                        catch (ExchangeDeadCardFailedException ex)
                        {
                            _logger.LogError(ex,
                                "#{Attempt}: Bot {Bot} produced invalid move ({Card}, {Coord}) in {GameId}",
                                attempt, botType.Name, card, coord, gameId);

                            continue;
                        }
                    }
                    else
                    {
                        try
                        {
                            gameEvents = await _playCardHandler.PlayCardAsync(gameId, playerId, card,
                                coord, cancellationToken);
                        }
                        catch (PlayCardFailedException ex)
                        {
                            _logger.LogError(ex,
                                "#{Attempt}: Bot {Bot} produced invalid move ({Card}, {Coord}) in {GameId}",
                                attempt, botType.Name, card, coord, gameId);

                            continue;
                        }
                    }

                    if (gameEvents != null)
                    {
                        _logger.LogInformation(
                            "#{Attempt}: Bot {Bot} produced a valid move for {GameId}",
                            attempt, botType.Name, gameId);

                        break;
                    }
                }

                if (gameEvents == null)
                {
                    _logger.LogWarning("Bot {Bot} failed to produce valid move", bot);
                }
            }
            else
            {
                _logger.LogWarning("Could not find bot type '{BotType}'", botTypeKey);
            }
        }
    }
}
