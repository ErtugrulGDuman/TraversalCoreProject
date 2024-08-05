using Microsoft.AspNetCore.SignalR;
using SignalRApiForSql.Models;

namespace SignalRApiForSql.Hubs
{
    public class VisitorHub : Hub
    {
        private readonly VisitorService _visitorService;
        public VisitorHub(VisitorService visitorService)
        {
            _visitorService = visitorService;
        }

        public async Task GetVisitorList()
        {
            try
            {
                var visitorList = _visitorService.GetVisitorChartList();
                await Clients.All.SendAsync("ReceiveVisitorList", visitorList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetVisitorList: {ex.Message}");
                throw; // Hatanın istemciye iletilmesi için
            }
        }
    }
}
