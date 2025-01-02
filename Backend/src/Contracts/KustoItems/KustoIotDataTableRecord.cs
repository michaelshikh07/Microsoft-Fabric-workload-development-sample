using System;
using System.Collections;
using System.Collections.Generic;

namespace Boilerplate.Contracts.KustoItems;

public class KustoIotDataTableRecord
{
    public DateTime Timestamp;
    public string Name;
    public double Value;
}

public static class KustoIotDataTableRecordExtensions
{
    private static readonly Random random = new Random();

    public static string ToCsvFormat(this KustoIotDataTableRecord record)
    {
        return string.Join(",", record.Timestamp, record.Name, record.Value);
    }

    public static KustoIotDataTableRecord GenerateRecord()
    {
        return new KustoIotDataTableRecord
        {
            Timestamp = DateTime.Now,
            Name = "sensor-" + random.Next(1, 200),
            Value = random.NextDouble()
        };
    }

    public static IEnumerable<KustoIotDataTableRecord> GenerateRecords(int numberOfRecords)
    {
        var records = new List<KustoIotDataTableRecord>(numberOfRecords);
        for (var i = 0; i < numberOfRecords; i++)
        {
            records.Add(GenerateRecord());
        }

        return records;
    }
}