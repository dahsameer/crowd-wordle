using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace CrowdWordle.Services;

public sealed class ConnectionManager(VotingService votingService)
{
    private readonly ConcurrentDictionary<uint, WebSocketConnection> _connections = [];

    public async Task HandleConnectionAsync(uint userId, WebSocket webSocket, GameEngine gameEngine)
    {
        if (_connections.TryRemove(userId, out var existingConnection))
        {
            _ = existingConnection.CloseAsync();
        }

        var connection = new WebSocketConnection(userId, webSocket);
        _connections[userId] = connection;

        try
        {
            SendInitialState(userId, gameEngine);
            await HandleClientMessages(connection, gameEngine);
        }
        catch(Exception)
        {
        }
        finally
        {
            await CloseConnection(userId, connection);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ConcurrentDictionary<uint, WebSocketConnection> GetAllConnections() => _connections;

    public Task BroadcastAsync(ReadOnlyMemory<byte> message)
    {
        var connections = _connections.Values;
        var tasks = new List<Task>(_connections.Count);

        foreach (var connection in connections)
        {
            if (connection.IsOpen)
            {
                tasks.Add(connection.SendAsync(message));
            }
        }

        return tasks.Count > 0 ? Task.WhenAll(tasks) : Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SendToUserAsync(uint userId, ReadOnlyMemory<byte> message)
    {
        return _connections.TryGetValue(userId, out var connection)
            ? connection.SendAsync(message)
            : Task.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetConnectionCount() => (uint)_connections.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsConnected(uint userId) => _connections.ContainsKey(userId);

    private async Task HandleClientMessages(WebSocketConnection connection, GameEngine gameEngine)
    {
        var buffer = new byte[4];

        while (connection.IsOpen)
        {
            var result = await connection.WebSocket.ReceiveAsync(buffer.AsMemory(), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
                break;

            if (result.MessageType == WebSocketMessageType.Binary && result.Count == 4)
            {
                var word = BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan());
                ref var game = ref gameEngine.GetCurrentGame();
                var response = votingService.Vote(connection.UserId, word, ref game, out bool first);
                if(first)
                {
                    var votingEndTime = votingService.GetVotingEndTime();
                    var message = EncodingHelper.PackVotingStarted(game, votingEndTime);
                    _ = BroadcastAsync(message.AsMemory());
                }
                var responseData = EncodingHelper.PackAcknowledgement(response);
                await connection.SendAsync(responseData.AsMemory());
            }
        }
    }

    private void SendInitialState(uint userId, GameEngine gameEngine)
    {
        ref readonly var game = ref gameEngine.GetCurrentGame();
        var totalVotes = votingService.GetTotalVotes();
        var topVotes = votingService.GetTop3Votes();
        var nextEventTime = gameEngine.GetNextEventTime() ?? votingService.GetVotingEndTime();
        var userVote = votingService.GetUserVote(userId);

        var message = EncodingHelper.PackInitialMessage(
            in game,
            nextEventTime,
            GetConnectionCount(),
            totalVotes,
            topVotes.AsSpan(),
            userVote);

        _ = SendToUserAsync(userId, message.AsMemory());
    }

    public async Task CloseConnection(uint userId, WebSocketConnection connection)
    {
        _connections.TryRemove(userId, out _);
        await connection.CloseAsync();
    }
}

public sealed class WebSocketConnection(uint userId, WebSocket webSocket)
{
    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);

    public uint UserId { get; } = userId;
    public WebSocket WebSocket { get; } = webSocket;
    public bool IsOpen => WebSocket.State == WebSocketState.Open;

    public async Task SendAsync(ReadOnlyMemory<byte> message)
    {
        if (!IsOpen) return;

        await _sendSemaphore.WaitAsync();
        try
        {
            if (IsOpen)
            {
                await WebSocket.SendAsync(message, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
        finally
        {
            _sendSemaphore.Release();
        }
    }

    public async Task CloseAsync()
    {
        if (WebSocket.State == WebSocketState.Open)
        {
            try
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            catch { }
        }
        _sendSemaphore.Dispose();
    }
}