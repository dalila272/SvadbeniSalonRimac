using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Services.Interfaces
{
    public interface ICRUDService<TEntity, T, TInsert>
    {
        T Insert(TInsert request);

        //   T Update(Guid id, TUpdate request);
    }
}
