using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvadbeniSalon.WinUI.Validators
{
    class EmptyFieldValidator : IDataErrorInfo
    {
        public string this[string columnName] => throw new NotImplementedException();

        public string Error => throw new NotImplementedException();
    }
}
