using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CabETL.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CabETL.EtlServices
{
    public class DeduplicationService
    {
        public IEnumerable<ProcessedTripRow> FilterUniques(IEnumerable<ProcessedTripRow> source, string duplicatesCsvPath)
        {
            var uniqueKeys = new HashSet<(DateTime PickupUtc, DateTime DropoffUtc, byte PassengerCount)>();

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using var writer = new StreamWriter(duplicatesCsvPath);
            using var csv = new CsvWriter(writer, csvConfig);

            csv.WriteHeader<ProcessedTripRow>();
            csv.NextRecord();

            foreach (var trip in source)
            {
                var key = (trip.PickupUtc, trip.DropoffUtc, trip.PassengerCount);

                if (uniqueKeys.Add(key))
                {
                    yield return trip;
                }
                else
                {
                    csv.WriteRecord(trip);
                    csv.NextRecord();
                }
            }
        }
    }
}

