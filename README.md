# Taxi ETL CLI (C#)

## Overview

This project is a simple ETL (Extract, Transform, Load) CLI application written in C#. It processes taxi trip data from a CSV file and loads it into a SQL Server database. The application focuses on efficient data processing, validation, and bulk insertion, while handling potentially unsafe input data.

---

## Features

- Streaming CSV processing (no full file load into memory)
- Data validation and cleaning
- Duplicate detection and export to `duplicates.csv`
- Conversion of `store_and_fwd_flag` values (`Y/N` → `Yes/No`)
- Trimming of all text fields
- Timezone conversion from EST to UTC
- Bulk insert into SQL Server using `SqlBulkCopy`
- Optimized database schema and indexes for analytical queries

---

## Technologies Used

- C#
- .NET (CLI application)
- SQL Server
- ADO.NET (`SqlConnection`, `SqlBulkCopy`)
- CsvHelper

---

## How It Works

1. Reads CSV file in a streaming manner  
2. Parses and validates each record  
3. Converts:
   - Dates (EST → UTC)
   - `Y/N` → `Yes/No`  
4. Trims all text values  
5. Writes duplicates into `duplicates.csv`  
6. Inserts valid records into SQL Server using batch bulk insert  

---

## Handling Unsafe Data

The application assumes the CSV source is unsafe and applies:

- `TryParse` for all numeric and date fields  
- Explicit date format parsing  
- Range validation (e.g. no negative values)  
- Skipping malformed records  
- No dynamic SQL (prevents injection)  

---

## Output

- Cleaned data inserted into `TaxiTrips` table  
- Duplicate records saved to `duplicates.csv`  
- Total inserted rows can be checked with:
