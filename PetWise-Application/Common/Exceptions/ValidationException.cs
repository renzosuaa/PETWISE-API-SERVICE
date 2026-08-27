using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
