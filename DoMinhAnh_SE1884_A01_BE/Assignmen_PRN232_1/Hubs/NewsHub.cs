using Microsoft.AspNetCore.SignalR;

namespace Assignmen_PRN232_1.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time news article notifications.
    /// Groups:
    ///   "public"  - all anonymous/lecturer viewers (join on connect)
    ///   "staff"   - authenticated staff members
    ///   "admin"   - admin users
    /// </summary>
    public class NewsHub : Hub
    {
        /// <summary>Called when a client connects. Joins the "public" group by default.</summary>
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "public");
            await base.OnConnectedAsync();
        }

        /// <summary>Client calls this to join an authenticated group (staff/admin).</summary>
        public async Task JoinGroup(string groupName)
        {
            var allowed = new[] { "staff", "admin", "public" };
            if (allowed.Contains(groupName.ToLower()))
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName.ToLower());
        }

        /// <summary>Client calls this to leave a group.</summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName.ToLower());
        }
    }

    // ─── Data shapes sent to clients ──────────────────────────────────────────

    public class ArticleNotification
    {
        public string Action { get; set; } = "";        // "created" | "updated" | "deleted"
        public string NewsArticleId { get; set; } = "";
        public string? NewsTitle { get; set; }
        public string? CategoryName { get; set; }
        public string? AuthorName { get; set; }
        public bool? NewsStatus { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ViewCountNotification
    {
        public string NewsArticleId { get; set; } = "";
        public int ViewCount { get; set; }
    }
}
