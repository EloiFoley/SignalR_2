using Microsoft.AspNetCore.Identity;

namespace signalr.backend.Models
{
    public class Channel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int NbMessages { get; set; }
    }
}
