using ASC.Web.Models;
using System.Threading.Tasks;

namespace ASC.Web.Data
{
    public interface INavigationCacheOperations
    {
        Task<NavigationMenu> GetNavigationCacheAsync();
        Task CreateNavigationCacheAsync();
    }
}