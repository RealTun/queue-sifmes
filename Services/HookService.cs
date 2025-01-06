using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QueueSifmes.Services
{
    public class HookService
    {
        //private readonly static string hookUrl = "http://192.168.3.108:8000/receive";
        private readonly static string hookUrl = "http://localhost:8000/receive";
        public static async Task SendDataAsync(object data)
        {
            using (var httpClient = new HttpClient())
            {
                // Chuyển đối tượng thành JSON
                var jsonData = JsonSerializer.Serialize(data);

                // Tạo HttpContent từ JSON
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Gửi POST request
                await httpClient.PostAsync(hookUrl, content);
            }
        }
    }
}
