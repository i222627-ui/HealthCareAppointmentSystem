using Microsoft.AspNetCore.SignalR;

namespace HealthCareAppointmentSystem.Hubs
{
    public class TelehealthHub : Hub
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> ConnectionToRoom = new();

        public async Task JoinRoom(string roomId)
        {
            ConnectionToRoom[Context.ConnectionId] = roomId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            // Notify others in the room that a user joined
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task LeaveRoom(string roomId)
        {
            ConnectionToRoom.TryRemove(Context.ConnectionId, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.OthersInGroup(roomId).SendAsync("UserLeft", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionToRoom.TryRemove(Context.ConnectionId, out var roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("UserLeft", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOffer(string roomId, string offer)
        {
            // The sender is offering their stream to others
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        public async Task SendAnswer(string roomId, string answer)
        {
            // The receiver is answering
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIceCandidate(string roomId, string candidate)
        {
            // Exchange ICE candidates for WebRTC peer-to-peer connection
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }
    }
}
