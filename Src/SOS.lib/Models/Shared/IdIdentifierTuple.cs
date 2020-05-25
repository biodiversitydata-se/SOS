using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// Id and identifier for data provider.
    /// </summary>
    public class IdIdentifierTuple : IIdIdentifierTuple
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
    }
}
