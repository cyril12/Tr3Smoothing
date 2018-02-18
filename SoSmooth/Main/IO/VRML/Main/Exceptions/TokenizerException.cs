﻿using System;

namespace SoSmooth.IO.Vrml
{
    public class TokenizerException : Exception
    {
        public TokenizerException() { }
        public TokenizerException(string message) : base(message) { }
        public TokenizerException(string message, Exception inner) : base(message, inner) { }
    }
}
