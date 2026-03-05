using System;

namespace CabETL.Models
{
    public class ProcessedTripRow
    {
        public DateTime PickupUtc { get; set; }
        public DateTime DropoffUtc { get; set; }
        public byte PassengerCount { get; set; }
        public decimal TripDistance { get; set; }
        public string StoreAndFwdFlag { get; set; }
        public int PULocationID { get; set; }
        public int DOLocationID { get; set; }
        public decimal FareAmount { get; set; }
        public decimal TipAmount { get; set; }
    }
}
