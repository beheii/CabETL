using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CabETL.Interfaces;
using CabETL.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CabETL.EtlServices
{
    public class DataReaderService : IDataReaderService
    {
        public IEnumerable<RawTripRow> Read(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            foreach (var record in csv.GetRecords<RawTripRow>())
            {
                yield return record;
            }
        }
    }
}
