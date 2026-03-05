using System;
using System.IO;
using CabETL.DataAccess;
using CabETL.EtlServices;
using CabETL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CabETL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: dotnet run -- \"<path-to-cabData.csv>\" \"<path-to-duplicates.csv>\"");
                    return;
                }

                var csvPath = args[0];
                var duplicatesCsvPath = args[1];

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionStringFromArgs = args.Length > 2 ? args[2] : null;
                var connectionStringFromEnv = Environment.GetEnvironmentVariable("CAB_ETL_CONNECTION");
                var connectionStringFromConfig = configuration.GetConnectionString("CabEtlDb");

                var connectionString =
                    connectionStringFromArgs
                    ?? connectionStringFromEnv
                    ?? connectionStringFromConfig
                    ?? throw new InvalidOperationException("Connection string is not configured. Provide it via args, CAB_ETL_CONNECTION env var, or appsettings.json.");

                var services = new ServiceCollection();

                services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
                services.AddTransient<IDataReaderService, DataReaderService>();
                services.AddTransient<IDataParserService, DataParserService>();
                services.AddTransient<DeduplicationService>();
                services.AddTransient<IBulkInsertService, BulkInsertService>();
                services.AddTransient<IEtlRunnerService, EtlRunnerService>();

                var provider = services.BuildServiceProvider();

                Console.WriteLine("CabETL starting...");
                Console.WriteLine($"Input CSV: {csvPath}");
                Console.WriteLine($"Duplicates CSV: {duplicatesCsvPath}");

                if (!File.Exists(csvPath))
                {
                    Console.WriteLine("Input CSV file not found.");
                    return;
                }

                var runner = provider.GetRequiredService<IEtlRunnerService>();
                var inserted = runner.Run(csvPath, duplicatesCsvPath);

                Console.WriteLine($"ETL completed. Inserted rows: {inserted}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ETL failed: " + ex.Message);
            }
        }
    }
}

