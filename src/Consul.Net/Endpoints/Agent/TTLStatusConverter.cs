using System;
using Newtonsoft.Json;

namespace Consul.Net.Endpoints.Agent
{
  public class TTLStatusConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      serializer.Serialize(writer, ((TTLStatus)value).Status);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
      JsonSerializer serializer)
    {
      var status = (string)serializer.Deserialize(reader, typeof(string));
      switch (status)
      {
        case "pass":
          return TTLStatus.Pass;
        case "passing":
          return TTLStatus.Pass;
        case "warn":
          return TTLStatus.Warn;
        case "warning":
          return TTLStatus.Warn;
        case "fail":
          return TTLStatus.Critical;
        case "critical":
          return TTLStatus.Critical;
        default:
          throw new ArgumentException("Invalid TTL status value during deserialization");
      }
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(TTLStatus);
    }
  }
}