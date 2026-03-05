using System.Collections.Generic;
using CabETL.Models;

namespace CabETL.Interfaces
{
    public interface IBulkInsertService
    {
        int Insert(IEnumerable<ProcessedTripRow> trips);
    }
}

