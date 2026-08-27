using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Exceptions
{
    public class TooManyRequestsException : Exception
    {
        public TooManyRequestsException(string message) : base(message) { }
    }
}
