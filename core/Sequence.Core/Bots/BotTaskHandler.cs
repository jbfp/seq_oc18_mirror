using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Core.Bots
{
    public sealed class BotTaskHandler
    {
        private readonly IGameProvider _provider;
        private readonly IGameEventStore _store;
        private readonly ILogger _logger;

        public BotTaskHandler(
            IGameProvider provider,
            IGameEventStore store,
            ILogger<BotTaskHandler> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _store = store ?? throw new ArgumentNullException(nameof(store));
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
            var game = await _provider.GetGameByIdAsync(gameId, cancellationToken);

            if (game == null)
            {
                _logger.LogWarning("Game with ID {GameId} could not be found for bot task", gameId);
            }
            else
            {
                var player = botTask.Player;
                var botTypeKey = player.Handle.ToString();

                if (BotProvider.BotTypes.TryGetValue(botTypeKey, out var botType))
                {
                    var bot = (IBot)Activator.CreateInstance(botType, nonPublic: true);
                    var gameView = game.GetViewForPlayer(player.Id);
                    var moves = game.GetMovesForPlayer(player.Id);

                    GameEvent gameEvent = null;

                    // Make it look like the bot is thinking... :)
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    for (int attempt = 1; attempt <= 10; attempt++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Move move = null;

                        try
                        {
                            move = bot.Decide(gameView, moves);
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

                        try
                        {
                            gameEvent = game.PlayCard(player.Id, card, coord);
                        }
                        catch (PlayCardFailedException ex)
                        {
                            _logger.LogError(ex,
                                "#{Attempt}: Bot {Bot} produced invalid move ({Card}, {Coord}) in {GameId}",
                                attempt, botType.Name, card, coord, gameId);

                            continue;
                        }

                        if (gameEvent != null)
                        {
                            _logger.LogInformation(
                                "#{Attempt}: Bot {Bot} produced a valid move for {GameId}",
                                attempt, botType.Name, gameId);

                            break;
                        }
                    }

                    if (gameEvent == null)
                    {
                        _logger.LogWarning("Bot {Bot} failed to produce valid move", bot);
                    }
                    else
                    {
                        await _store.AddEventAsync(gameId, gameEvent, cancellationToken);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not find bot type '{BotType}'", botTypeKey);
                }
            }
        }
    }
}
