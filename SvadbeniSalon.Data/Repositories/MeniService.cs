using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class MeniService : BaseCRUDService<Models.Meni, Meni>
    {
        public MeniService(LocalContext context, IMapper mapper) : base(context, mapper)
        {

        }


        public override IList<Shared.Models.Meni> GetAll()
        {
            var result = _context.Meniji.Include(m => m.Artikli).ThenInclude(ma => ma.Artikal).ToList();
            return _mapper.Map<List<Meni>>(result);
        }


        public override Shared.Models.Meni GetById(long id)
        {
            var result = _context.Meniji.Include(m => m.Artikli).ThenInclude(ma => ma.Artikal).FirstOrDefault(x => x.Id == id);
            return _mapper.Map<Meni>(result);
        }
    }
}
