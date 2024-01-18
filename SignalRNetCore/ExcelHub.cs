using Microsoft.AspNetCore.SignalR;

namespace SignalRNetCore
{
    public sealed class ExcelHub : Hub
    {
        public async Task SendProgressUpdate(string message)
        {
            await Clients.All.SendAsync("ProgressChanged", message);
        }
    }
}
