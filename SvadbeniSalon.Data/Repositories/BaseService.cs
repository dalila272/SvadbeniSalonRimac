using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class BaseService<TEntity, TModel> where TEntity : class
    {
        protected readonly LocalContext _context;
        protected readonly IMapper _mapper;
        internal DbSet<TEntity> dbSet;

        public BaseService(LocalContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            dbSet = _context.Set<TEntity>();

        }
        public virtual IList<TModel> GetAll()
        {
            var result = _context.Set<TEntity>().ToList();
            return _mapper.Map<IList<TModel>>(result);
        }

        public virtual TModel GetById(long id)
        {
            var entity = _context.Set<TEntity>().Find(id);
            return _mapper.Map<TModel>(entity);
        }
    }
}
