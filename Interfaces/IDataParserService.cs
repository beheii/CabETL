using CabETL.Models;

namespace CabETL.Interfaces
{
    public interface IDataParserService
    {
        ProcessedTripRow? Transform(RawTripRow raw);
    }
}

