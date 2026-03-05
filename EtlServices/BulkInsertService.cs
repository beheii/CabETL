using System.Collections.Generic;
using System.Data;
using CabETL.DataAccess;
using CabETL.Interfaces;
using CabETL.Models;
using Microsoft.Data.SqlClient;

namespace CabETL.EtlServices
{
    public class BulkInsertService : IBulkInsertService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly int _batchSize;

        public BulkInsertService(IDbConnectionFactory connectionFactory, int batchSize = 10000)
        {
            _connectionFactory = connectionFactory;
            _batchSize = batchSize;
        }

        public int Insert(IEnumerable<ProcessedTripRow> trips)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "dbo.Trips",
                BatchSize = _batchSize,
                BulkCopyTimeout = 0
            };

            bulkCopy.ColumnMappings.Add("tpep_pickup_datetime", "tpep_pickup_datetime");
            bulkCopy.ColumnMappings.Add("tpep_dropoff_datetime", "tpep_dropoff_datetime");
            bulkCopy.ColumnMappings.Add("passenger_count", "passenger_count");
            bulkCopy.ColumnMappings.Add("trip_distance", "trip_distance");
            bulkCopy.ColumnMappings.Add("store_and_fwd_flag", "store_and_fwd_flag");
            bulkCopy.ColumnMappings.Add("PULocationID", "PULocationID");
            bulkCopy.ColumnMappings.Add("DOLocationID", "DOLocationID");
            bulkCopy.ColumnMappings.Add("fare_amount", "fare_amount");
            bulkCopy.ColumnMappings.Add("tip_amount", "tip_amount");

            var table = CreateTripsDataTable();
            var insertedCount = 0;

            foreach (var trip in trips)
            {
                var row = table.NewRow();
                row["tpep_pickup_datetime"] = trip.PickupUtc;
                row["tpep_dropoff_datetime"] = trip.DropoffUtc;
                row["passenger_count"] = trip.PassengerCount;
                row["trip_distance"] = trip.TripDistance;
                row["store_and_fwd_flag"] = trip.StoreAndFwdFlag;
                row["PULocationID"] = trip.PULocationID;
                row["DOLocationID"] = trip.DOLocationID;
                row["fare_amount"] = trip.FareAmount;
                row["tip_amount"] = trip.TipAmount;

                table.Rows.Add(row);

                if (table.Rows.Count >= _batchSize)
                {
                    bulkCopy.WriteToServer(table);
                    insertedCount += table.Rows.Count;
                    table.Clear();
                }
            }

            if (table.Rows.Count > 0)
            {
                bulkCopy.WriteToServer(table);
                insertedCount += table.Rows.Count;
                table.Clear();
            }

            return insertedCount;
        }

        private static DataTable CreateTripsDataTable()
        {
            var table = new DataTable();

            table.Columns.Add("tpep_pickup_datetime", typeof(DateTime));
            table.Columns.Add("tpep_dropoff_datetime", typeof(DateTime));
            table.Columns.Add("passenger_count", typeof(byte));
            table.Columns.Add("trip_distance", typeof(decimal));
            table.Columns.Add("store_and_fwd_flag", typeof(string));
            table.Columns.Add("PULocationID", typeof(int));
            table.Columns.Add("DOLocationID", typeof(int));
            table.Columns.Add("fare_amount", typeof(decimal));
            table.Columns.Add("tip_amount", typeof(decimal));

            return table;
        }
    }
}

