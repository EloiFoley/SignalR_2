using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using signalr.backend.Data;
using signalr.backend.Models;

namespace signalr.backend.Hubs
{
    public class UserData
    {
        public string UserId { get; set; }
        public int NbConnexions { get; set; }
    }

    // On garde en mémoire les connexions actives (clé: email, valeur: userId)
    // Note: Ce n'est pas nécessaire dans le TP
    public static class UserHandler
    {
        public static Dictionary<string, UserData> UserConnections { get; set; } = new Dictionary<string, UserData>();
    }

    // L'annotation Authorize fonctionne de la même façon avec SignalR qu'avec Web API
    [Authorize]
    // Le Hub est le type de base des "contrôleurs" de SignalR
    public class ChatHub : Hub
    {
        public ApplicationDbContext _context;

        public IdentityUser CurentUser
        {
            get
            {
                // On récupère le userid à partir du Cookie qui devrait être envoyé automatiquement
                string userid = Context.UserIdentifier!;
                return _context.Users.Single(u => u.Id == userid);
            }
        }

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        private void AugmenterNbConnexions()
        {
            if (!UserHandler.UserConnections.ContainsKey(CurentUser.UserName!))
            {
                UserHandler.UserConnections[CurentUser.UserName!] = new UserData() { UserId = Context.UserIdentifier! };
            }
            UserHandler.UserConnections[CurentUser.UserName!].NbConnexions++;
        }

        private void ReduireNbConnexions()
        {
            UserData data = UserHandler.UserConnections[CurentUser.UserName!];
            data.NbConnexions--;
            if (data.NbConnexions == 0)
                UserHandler.UserConnections.Remove(CurentUser.UserName!);
        }

        public async override Task OnConnectedAsync()
        {
            // Lors de la connexion, on augmente le nombre de connexions pour cet usager dans le dictionnaire
            AugmenterNbConnexions();

            // TODO: Envoyer des message aux clients pour les mettre à jour
            await Clients.All.SendAsync("UsersList", UserHandler.UserConnections.ToList());
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            // Lors de la fermeture de la connexion, on met à jour notre dictionnary d'utilisateurs connectés
            ReduireNbConnexions();

            // TODO: Envoyer un message aux clients pour les mettre à jour
            await Clients.All.SendAsync("UsersList", UserHandler.UserConnections.ToList());
        }

        public async Task CreateChannel(string title)
        {
            _context.Channel.Add(new Channel { Title = title });
            await _context.SaveChangesAsync();

            // TODO: Envoyer un message aux clients pour les mettre à jour
        }

        public async Task DeleteChannel(int channelId)
        {
            Channel channel = _context.Channel.Find(channelId);

            if(channel != null)
            {
                _context.Channel.Remove(channel);
                await _context.SaveChangesAsync();
            }
            string groupName = CreateChannelGroupName(channelId);
            // Envoyer les messages nécessaires aux clients
        }

        public async Task JoinChannel(int oldChannelId, int newChannelId)
        {
            string userTag = "[" + CurentUser.Email! + "]";

            // TODO: Faire quitter le vieux canal à l'utilisateur

            // TODO: Faire joindre le nouveau canal à l'utilisateur
        }

        public async Task SendMessage(string message, int channelId, string userId)
        {
            if (userId != null)
            {
                // TODO: Envoyer le message à cet utilisateur
            }
            else if (channelId != 0)
            {
                // TODO: Envoyer le message aux utilisateurs connectés à ce canal
            }
            else
            {
                await Clients.All.SendAsync("NewMessage", "[Tous] " + message);
            }
        }

        private static string CreateChannelGroupName(int channelId)
        {
            return "Channel" + channelId;
        }
    }
}