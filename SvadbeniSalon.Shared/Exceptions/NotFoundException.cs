using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Shared.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string type) : base($"Entity of type {type} cannot not found")
        {

        }
    }
}
