using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sequence.Core;
using Sequence.Core.CreateGame;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sequence.Api
{
    public sealed class CreateGameController : SequenceControllerBase
    {
        private readonly CreateGameHandler _handler;
        private readonly ILogger _logger;

        public CreateGameController(CreateGameHandler handler, ILogger<CreateGameController> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("/games")]
        public async Task<ActionResult> Post(
            [FromBody] CreateGameForm form,
            CancellationToken cancellationToken)
        {
            var players = form.Opponents
                .Select(opponent => new NewPlayer(new PlayerHandle(opponent.Name), opponent.Type.Value))
                .Prepend(new NewPlayer(Player, PlayerType.User))
                .ToArray();

            PlayerList playerList;

            _logger.LogInformation("Attempting to create game for {Players}", players);

            try
            {
                playerList = new PlayerList(players);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogWarning(ex, "Game size invalid for {Players}", players);
                return BadRequest(new { error = "Game size is invalid." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Duplicate players in {Players}", players);
                return BadRequest(new { error = "Duplicate players are not allowed." });
            }

            var boardType = form.BoardType.Value;
            var gameId = await _handler.CreateGameAsync(playerList, boardType, cancellationToken);

            _logger.LogInformation(
                "Successfully created game with ID {GameId} for {Players}",
                gameId, players);

            return Created($"/games/{gameId}", new { gameId });
        }
    }

    public sealed class CreateGameForm
    {
        [Required, Enum(typeof(BoardType))]
        public BoardType? BoardType { get; set; }

        [Required]
        public Opponent[] Opponents { get; set; }
    }

    public sealed class Opponent : IValidatableObject
    {
        [Required]
        public PlayerType? Type { get; set; }

        [Required]
        public string Name { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is Opponent opponent)
            {
                if (opponent.Type == PlayerType.Bot)
                {
                    var botType = opponent.Name;

                    if (!BotProvider.BotTypes.ContainsKey(opponent.Name))
                    {
                        var errorMessage = $"The bot type '{opponent.Name}' does not exist.";
                        var memberNames = new[] { nameof(Name) };
                        yield return new ValidationResult(errorMessage, memberNames);
                    }
                }
            }
        }
    }
}
