using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Interfaces
{
    /// <summary>
    /// Data provider id and identifier.
    /// </summary>
    public interface IIdIdentifierTuple
    {
        int Id { get; }
        string Identifier { get; }
    }
}