using SvadbeniSalon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Models.Requests.Artikal
{
    public class FilterArtikal
    {
        public TipArtikla? Tip { get; set; }
        public string Naziv { get; set; }
    }
}
