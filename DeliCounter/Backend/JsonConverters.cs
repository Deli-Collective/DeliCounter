using Newtonsoft.Json;
using System;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace DeliCounter.Backend
{
    public class SemVersionConverter : JsonConverter<Version>
    {
        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Version.Parse((string)reader.Value);
        }
    }

    public class SemRangeConverter : JsonConverter<Range>
    {
        public override void WriteJson(JsonWriter writer, Range value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Range ReadJson(JsonReader reader, Type objectType, Range existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Range.Parse((string)reader.Value);
        }
    }
}