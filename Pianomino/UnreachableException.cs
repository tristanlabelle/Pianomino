using System;
using System.Collections.Generic;
using System.Text;

namespace Pianomino;

internal sealed class UnreachableException : Exception
{
    public UnreachableException() { }
    public UnreachableException(string message) : base(message) { }
}
