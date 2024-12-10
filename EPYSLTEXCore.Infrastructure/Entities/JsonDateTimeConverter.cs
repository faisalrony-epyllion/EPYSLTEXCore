using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _dateFormat;

    // Constructor to accept the desired date format
    public JsonDateTimeConverter(string dateFormat)
    {
        _dateFormat = dateFormat;
    }

    // Read the DateTime string and parse it
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string dateString = reader.GetString();
        return DateTime.ParseExact(dateString, _dateFormat, CultureInfo.InvariantCulture);
    }

    // Write the DateTime value in the desired format
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_dateFormat));
    }
}

