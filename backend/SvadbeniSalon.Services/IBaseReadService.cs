using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SvadbeniSalon.Services
{
    public interface IBaseReadService<TResponse, TSearch>
        where TSearch : BaseSearchObject
    {
        Task<TResponse> GetByIdAsync(int id);
        Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null);
    }
}
