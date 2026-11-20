using Microsoft.AspNetCore.SignalR;
using signalr.backend.Data;
using signalr.backend.Hubs;

namespace signalr.backend.Services
{
    public class ChatBackgroundService : BackgroundService
    {
        public const int DELAY = 30 * 1000;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHubContext<ChatHub> _chatHubContext;

        public ChatBackgroundService(IServiceScopeFactory serviceScopeFactory, IHubContext<ChatHub> chatHubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _chatHubContext = chatHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    int nbMessagesOfMostPopularChannel = dbContext.Channel.Select(c => c.NbMessages).Max();

                    if (nbMessagesOfMostPopularChannel > 0)
                    {
                        var mostPopularChannels = dbContext.Channel.Where(c => c.NbMessages >= nbMessagesOfMostPopularChannel).ToList();

                        var mostPopularChannelGroups = mostPopularChannels.Select(c => ChatHub.CreateChannelGroupName(c.Id));

                        await _chatHubContext.Clients.Groups(mostPopularChannelGroups).SendAsync("MostPopularChannel", nbMessagesOfMostPopularChannel, stoppingToken);
                    }
                }
                await Task.Delay(DELAY);
            }
        }
    }
}
