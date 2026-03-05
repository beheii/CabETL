# ETL Program for Trip Data Processing

 After running the program, the table contains **26,805 rows**.  

## Schema Optimization

### Highest Average Tip by `PULocationID`
- Added a nonclustered index on `PULocationID`.  
- **With index:** 16 ms CPU / 66 ms elapsed, using a **stream aggregate**.  
- **Without index:** 32 ms CPU / 134 ms elapsed, using a **hash aggregate**.  
- The index improves aggregation performance significantly.  

### Top 100 Longest Trips by `trip_distance`
- Added a nonclustered index on `trip_distance`.  
- **With index:** 15 ms CPU / 226 ms elapsed, using **nested loops** with **key lookups**.  
- **Without index:** 63 ms CPU / 443 ms elapsed, performing a full table scan.  

### Top 100 Longest Trips by Duration
- Added a nonclustered index on the computed `trip_duration_minutes` column.  
- **With index:** 0 ms CPU / 285 ms elapsed, using **nested loops** with an **index scan**.  
- **Without index:** 62 ms CPU / 490 ms elapsed, scanning the entire table.  

### Why a Wide Covering Index Was Not Added
- A nonclustered index on `PULocationID` alone does not significantly speed up queries like `SELECT * FROM dbo.Trips WHERE PULocationID = 40`.  
- A full covering index would avoid lookups but would almost duplicate the table, increasing storage and write costs.  
- Since exact read patterns are unknown, the simple index is retained, accepting current performance.  
---

## Data Handling

- Input data is converted to **UTC** when inserted into the database.  
- Each row is validated field by field:  
  - Numeric values use `TryParse`; invalid or out-of-range rows are skipped.  
  - String fields are trimmed and checked against basic business rules.  
- Only cleaned records are bulk inserted into SQL Server via `SqlBulkCopy`, minimizing SQL injection risks.  
---

## Business Rules and Assumptions

- **Trip distance:** Interpreted as kilometers; rows with `trip_distance < 0.5` are removed. This threshold was chosen based on data exploration (no significant spike was noticed at all). 
- **Passenger count:** Must be `>= 1`. A trip without at least one passenger is considered invalid.
- **Fare amount:** Must be `> 0`.  A completed fare with a non positive amount is assumed to be invalid.
- **Tip amount:** Must be `>= 0`.  Tips can legitimately be zero or any positive value.
- **Trip duration:** Must be `<= 200` minutes; longer trips are treated as outliers. (there was a significant spike in data).
- **Pickup vs dropoff:** `PULocationID != DOLocationID`.  Trips that start and end in exactly the same zone are excluded.
- **Time ordering:** `tpep_pickup_datetime < tpep_dropoff_datetime`. A ride must start before it can finish; rows violating this are discarded.  
---

## Additional Considerations

- If we find price for ride per km for each row, we would be able to define which rows are outliers. The disadvantage is that we cannot decide for sure what price/km is reasonable and realistic. 
---

## Large Dataset Strategy

If the program is used on much larger files:  

- Avoid using an in-memory `HashSet` for duplicate removal; process data in smaller chunks, clearing memory after each.  
- Stream files line by line and switch to asynchronous I/O for reading and writing.  
- Increase bulk insert batch size and adjust database timeouts for better performance.  
---

## SQL Scripts

```sql
USE CabEtlDb;
GO

CREATE TABLE dbo.Trips
(
    TripId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    tpep_pickup_datetime DATETIME2(0) NOT NULL,   
    tpep_dropoff_datetime DATETIME2(0) NOT NULL,    
    passenger_count TINYINT NOT NULL,
    trip_distance DECIMAL(10,2) NOT NULL,
    store_and_fwd_flag VARCHAR(3) NOT NULL,   
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    fare_amount DECIMAL(10,2) NOT NULL,
    tip_amount DECIMAL(10,2) NOT NULL,
    -- to visualize and understand data better
    trip_duration_minutes AS DATEDIFF(MINUTE, tpep_pickup_datetime, tpep_dropoff_datetime) PERSISTED
);
GO

-- optimization for avg highest tip in PU search
CREATE NONCLUSTERED INDEX IX_Trips_PULocationID
ON dbo.Trips (PULocationID)
INCLUDE (tip_amount);
GO

-- optimization for top 100 longest by distance
CREATE NONCLUSTERED INDEX IX_Trips_TripDistance
ON dbo.Trips (trip_distance DESC);
GO

-- optimization for top 100 longest by time
CREATE NONCLUSTERED INDEX IX_Trips_TripDuration
ON dbo.Trips (trip_duration_minutes DESC);
GO