using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.WinUI.Interfaces
{
    public interface ICloseWindow
    {
        Action Close { get; set; }
    }
}
