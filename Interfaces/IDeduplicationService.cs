using System.Collections.Generic;
using CabETL.Models;

namespace CabETL.Interfaces
{
    public interface IDeduplicationService
    {
        IEnumerable<ProcessedTripRow> FilterUniques(IEnumerable<ProcessedTripRow> source, string duplicatesCsvPath);
    }
}
