using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class BaseCRUDService<TEntity, TModel> : BaseService<TEntity, TModel> where TEntity : class
    {

        public BaseCRUDService(LocalContext context, IMapper mapper) : base(context, mapper)
        {


        }

        public virtual TModel Add(TModel model)
        {
            var item = _mapper.Map<TModel, TEntity>(model);
            _context.Add(item);
            _context.SaveChanges();
            return _mapper.Map<TModel>(item);
        }


        public void Remove(object id)
        {
            var item = dbSet.Find(id);
            dbSet.Remove(item);
            _context.SaveChanges();
        }

        public TModel GetById(Guid id)
        {
            var item = dbSet.Find(id);

            return _mapper.Map<TModel>(item);
        }


    }
}
