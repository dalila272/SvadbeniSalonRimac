using AutoMapper;
using SvadbeniSalon.API.DTO.Requests.Meni;
using SvadbeniSalon.API.DTO.Requests.Muzicar;
using SvadbeniSalon.API.DTO.Requests.Ponuda;
using SvadbeniSalon.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Shared.Models.Korisnik, Database.Models.Korisnik>();
            CreateMap<Database.Models.Korisnik, Shared.Models.Korisnik>();


            CreateMap<Database.Models.Muzicar, Shared.Models.Muzicar>().ForMember(m => m.Zanrovi, z => z.MapFrom(m => m.Zanrovi.Select(x => x.Zanr)));
            CreateMap<Shared.Models.Muzicar, Muzicar>().ForMember(m => m.Zanrovi, zanrovi => zanrovi.MapFrom(m => m.Zanrovi.Select(x => new MuzicarZanr { ZanrId = x.Id })));
            CreateMap<AddMuzicar, Shared.Models.Muzicar>().ForMember(m => m.Zanrovi, req => req.MapFrom(x => x.Zanrovi.Select(x => new Zanr { Id = x })));

            CreateMap<Database.Models.Zanr, Shared.Models.Zanr>();
            CreateMap<Shared.Models.Zanr, Database.Models.Zanr>();

            CreateMap<Dekoracija, Shared.Models.Dekoracija>();
            CreateMap<Shared.Models.Dekoracija, Dekoracija>();
            CreateMap<Shared.Models.Requests.Dekoracija.AddDekoracija, Shared.Models.Dekoracija>();

            CreateMap<Artikal, Shared.Models.Artikal>();
            CreateMap<Shared.Models.Artikal, Artikal>();
            CreateMap<Shared.Models.Requests.Artikal.AddArtikal, Shared.Models.Artikal>();


            CreateMap<Meni, Shared.Models.Meni>().ForMember(m => m.Artikli, z => z.MapFrom(m => m.Artikli.Select(x => x.Artikal)));
            CreateMap<Shared.Models.Meni, Meni>().ForMember(m => m.Artikli, artikli => artikli.MapFrom(m => m.Artikli.Select(x => new MeniArtikal { ArtikalId = x.Id })));
            CreateMap<AddMeni, Shared.Models.Meni>().ForMember(m => m.Artikli, req => req.MapFrom(x => x.Artikli.Select(a => new Artikal { Id = a.Id })));


            CreateMap<AddPonuda, Shared.Models.Ponuda>().ForMember(p => p.Dekoracije, req => req.MapFrom(x => x.Dekoracije.Select(d => new Dekoracija { Id = d })))
                                                        .ForMember(p => p.Muzicari, req => req.MapFrom(x => x.Muzicari.Select(m => new Muzicar { Id = m })));

            CreateMap<Shared.Models.Ponuda, Ponuda>().ForMember(m => m.DekoracijePonuda, dekoracijePonuda => dekoracijePonuda.MapFrom(p => p.Dekoracije.Select(x => new DekoracijaPonuda { DekoracijaId = x.Id })))
                                                     .ForMember(m => m.MuzicariPonuda, muzicariPonuda => muzicariPonuda.MapFrom(p => p.Muzicari.Select(x => new MuzicariPonuda { MuzicarId = x.Id })));

            CreateMap<Ponuda, Shared.Models.Ponuda>().ForMember(p => p.Dekoracije, req => req.MapFrom(x => x.DekoracijePonuda.Select(dp => dp.Dekoracija)))
                                                        .ForMember(p => p.Muzicari, req => req.MapFrom(x => x.MuzicariPonuda.Select(mp => mp.Muzicar)));

        }
    }
}
