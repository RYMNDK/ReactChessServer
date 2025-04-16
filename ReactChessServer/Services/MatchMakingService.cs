using System.Collections.Concurrent;
using ReactChessServer.Models;

namespace ReactChessServer.Services;

/// <summary>
///     responsible for keeping a list of waiting games and a queue for getting waiting games
///     hand the game id to game manager service after a match
/// </summary>
public class MatchMakingService
{
    // todo: find a distributed unique key if guid has collision 
    // todo: add finer matchmaking (based on elo or something)
    private readonly ConcurrentQueue<Guid> _publicGameList = new();
    private readonly ConcurrentDictionary<Guid, bool> _validGameList = new();

    // adding a game to a matchmaking list
    public void Enqueue(Guid id) => _publicGameList.Enqueue(id);

    // Adding a game to the active games
    public void AddGame(Guid id) => _validGameList.TryAdd(id, true);

    /// <summary>
    ///     invalidate a waiting game, a game is invalidated if the room creator leaves
    ///     or another player entered with the same room id
    ///     This is a ONE WAY OPERATION
    /// </summary>
    /// <param name="id"></param>
    public void Invalidate(Guid id) => _validGameList.TryAdd(id, false);

    public List<Guid> GetActiveWaitingGameIds() => _validGameList.Keys
        .Where(gameId => 
            !_validGameList.TryGetValue(gameId, out bool isInvalid) || !isInvalid)
        .ToList();

    
    public Guid? GetWithGameId(Guid gameId)
    {
        // Check if the game exists and is valid (true)
        if (_validGameList.TryGetValue(gameId, out bool isValid) && isValid) {
            // Mark it as used (false)
            _validGameList.TryUpdate(gameId, false, true);
            return gameId;
        }
        return null;
    }
    
    // returning null if a waiting game was not found.
    public Guid? GetNextWaitingGameId()
    {
        Guid gameId;
        while (_publicGameList.TryDequeue(out gameId))
        {
            _validGameList.TryGetValue(gameId, out bool isInvalid);
            _validGameList.TryRemove(gameId, out _);
            if (isInvalid)
            {
                return gameId;
            }
        }
        
        return null;
    }
}

