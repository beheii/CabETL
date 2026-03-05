using System;
using System.Globalization;
using CabETL.Interfaces;
using CabETL.Models;

namespace CabETL.EtlServices
{
    public class DataParserService : IDataParserService
    {
        private const string DateTimeFormat = "MM/dd/yyyy hh:mm:ss tt";
        private readonly TimeZoneInfo _easternTimeZone;

        public DataParserService()
        {
            _easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        public ProcessedTripRow? Transform(RawTripRow raw)
        {
            try
            {
                if (!TryParsePickupDropoff(raw, _easternTimeZone, out var pickupUtc, out var dropoffUtc))
                    return null;

                var tripDurationMinutes = (dropoffUtc - pickupUtc).TotalMinutes;
                if (tripDurationMinutes > 200)
                    return null;

                if (!TryParseDecimal(raw.trip_distance, out var tripDistance) ||
                    !TryParseDecimal(raw.fare_amount, out var fareAmount) ||
                    !TryParseDecimal(raw.tip_amount, out var tipAmount) ||
                    !TryParseByte(raw.passenger_count, out var passengerCount) ||
                    !TryParseInt(raw.PULocationID, out var puLocationId) ||
                    !TryParseInt(raw.DOLocationID, out var doLocationId))
                    return null;

                if (puLocationId == doLocationId)
                    return null;

                var storeAndFwdFlag = NormalizeStoreAndFwdFlag(raw.store_and_fwd_flag);

                if (!IsValidRow(passengerCount, tripDistance, fareAmount, tipAmount, pickupUtc, dropoffUtc))
                    return null;

                return new ProcessedTripRow
                {
                    PickupUtc = pickupUtc,
                    DropoffUtc = dropoffUtc,
                    PassengerCount = passengerCount,
                    TripDistance = tripDistance,
                    StoreAndFwdFlag = storeAndFwdFlag,
                    PULocationID = puLocationId,
                    DOLocationID = doLocationId,
                    FareAmount = fareAmount,
                    TipAmount = tipAmount
                };
            }
            catch
            {
                return null;
            }
        }

        private static bool TryParsePickupDropoff(RawTripRow raw, TimeZoneInfo eastern, out DateTime pickupUtc, out DateTime dropoffUtc)
        {
            pickupUtc = default;
            dropoffUtc = default;

            if (raw.tpep_pickup_datetime is null || raw.tpep_dropoff_datetime is null)
                return false;

            try
            {
                var pickupLocal = DateTime.ParseExact(
                    raw.tpep_pickup_datetime.Trim(),
                    DateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None);

                var dropoffLocal = DateTime.ParseExact(
                    raw.tpep_dropoff_datetime.Trim(),
                    DateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None);

                pickupUtc = TimeZoneInfo.ConvertTimeToUtc(pickupLocal, eastern);
                dropoffUtc = TimeZoneInfo.ConvertTimeToUtc(dropoffLocal, eastern);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool TryParseDecimal(string? value, out decimal result)
        {
            return decimal.TryParse((value ?? "0").Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseInt(string? value, out int result)
        {
            return int.TryParse((value ?? "0").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseByte(string? value, out byte result)
        {
            return byte.TryParse((value ?? "0").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static string NormalizeStoreAndFwdFlag(string? value)
        {
            var flagRaw = (value ?? string.Empty).Trim().ToUpperInvariant();
            return flagRaw switch
            {
                "Y" => "Yes",
                "N" => "No",
                _ => "No"
            };
        }

        private static bool IsValidRow(byte passengerCount, decimal tripDistance, decimal fareAmount, decimal tipAmount, DateTime pickupUtc, DateTime dropoffUtc)
        {
            return passengerCount >= 1 && tripDistance >= 0.5m && fareAmount >= 1m && tipAmount >= 0 && dropoffUtc >= pickupUtc;
        }
    }
}
