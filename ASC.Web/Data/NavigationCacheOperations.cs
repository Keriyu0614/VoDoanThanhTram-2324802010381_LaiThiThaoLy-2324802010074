using ASC.Web.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ASC.Web.Data
{
    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly string NavigationCacheName = "NavigationCache";

        public NavigationCacheOperations(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task CreateNavigationCacheAsync()
        {
            // Đọc nội dung file JSON và lưu vào Cache dưới dạng chuỗi (String)
            await _cache.SetStringAsync(NavigationCacheName, File.ReadAllText("Navigation/Navigation.json"));
        }

        public async Task<NavigationMenu> GetNavigationCacheAsync()
        {
            // Lấy chuỗi JSON từ Cache và chuyển đổi ngược lại thành đối tượng NavigationMenu
            var cachedData = await _cache.GetStringAsync(NavigationCacheName);
            return JsonConvert.DeserializeObject<NavigationMenu>(cachedData);
        }
    }
}