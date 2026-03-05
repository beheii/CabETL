using System.Collections.Generic;
using CabETL.Models;

namespace CabETL.Interfaces
{
    public interface IDataReaderService
    {
        IEnumerable<RawTripRow> Read(string path);
    }
}

