using System;

namespace DataBaseGenerator.Core.OrthancWorklists;

public sealed class OrthancWorklistException : Exception
{
    public OrthancWorklistException(string message)
        : base(message)
    {
    }
}
