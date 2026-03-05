using System;
using System.Collections.Generic;
using System.Linq;
using CabETL.Interfaces;
using CabETL.Models;

namespace CabETL.EtlServices
{
    public class EtlRunnerService : IEtlRunnerService
    {
        private readonly IDataReaderService _reader;
        private readonly IDataParserService _parser;
        private readonly DeduplicationService _deduplicator;
        private readonly IBulkInsertService _bulkInserter;

        public EtlRunnerService(
            IDataReaderService reader,
            IDataParserService parser,
            DeduplicationService deduplicator,
            IBulkInsertService bulkInserter)
        {
            _reader = reader;
            _parser = parser;
            _deduplicator = deduplicator;
            _bulkInserter = bulkInserter;
        }

        public int Run(string csvPath, string duplicatesCsvPath)
        {
            IEnumerable<RawTripRow> rawRows = _reader.Read(csvPath);

            var transformed = rawRows
                .Select(r => _parser.Transform(r))
                .Where(trip => trip != null)!
                .Cast<ProcessedTripRow>();

            var uniques = _deduplicator.FilterUniques(transformed, duplicatesCsvPath);
            var insertedCount = _bulkInserter.Insert(uniques);

            return insertedCount;
        }
    }
}
