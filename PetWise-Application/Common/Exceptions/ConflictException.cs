using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
