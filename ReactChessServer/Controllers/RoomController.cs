using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ReactChessServer.Hubs;
using ReactChessServer.Services;

namespace ReactChessServer.Controllers;

// http endpoint is for debugging with swagger

[ApiController]
[Route("api/[controller]")]
public class RoomController(
    ILogger<RoomController> logger,
    IHubContext<GameHub> gameHub,
    MatchMakingService mms) : ControllerBase
{
    // GET: api/room/<ChessController>
    [HttpGet("{id:guid}")]
    public Guid Get(Guid id) => id;

    // POST api/room/NewGame
    [HttpPost("NewGame")]
    public Guid NewGame()
    {
        Guid id = Guid.NewGuid();
        mms.AddGame(id);
        mms.Enqueue(id);
        logger.LogInformation($"New game with id {id}, matchmaking started.");
        return id;
    }

    // Get in a existing name or get into a waiting game
    [HttpPost("EnterRoom")]
    public IActionResult EnterRoom(Guid? gameId)
    {
        // Get matched with next game
        var gameFoundId = gameId == null ? mms.GetNextWaitingGameId() : mms.GetWithGameId(gameId.Value);

        if (gameFoundId == null)
        {
            String message = gameId == null ? $"No waiting games found." : $"Game with id {gameId} not found.";
            return NotFound(message);
        }
        
        // Start a new game
        // Notify the other player
        return Ok("LET THE GAMES BEGIN!");
    }
    
    // Get api/room/mms/waiting, Do not consider invalid games
    [HttpGet("mms/waiting")]
    public List<Guid> GetWaitingGames() => mms.GetActiveWaitingGameIds();

    [HttpPost("/message")]
    public Task MessageAll(String message) => gameHub.Clients.All.SendAsync("MessageFromRest", message);
}