﻿using System;

namespace SoSmooth.IO.Vrml
{
    public class InvalidFieldException : VrmlParseException
    {
        public InvalidFieldException() { }
        public InvalidFieldException(string message) : base(message) { }
        public InvalidFieldException(string message, Exception inner) : base(message, inner) { }
    }
}
