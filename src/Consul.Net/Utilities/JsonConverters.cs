﻿using System;
using System.Linq;
using System.Reflection;
using Consul.Net.Endpoints.KV;
using Newtonsoft.Json;

namespace Consul.Net.Utilities
{
    public class NanoSecTimespanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, (long)((TimeSpan)value).TotalMilliseconds * 1000000, typeof(long));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return Extensions.FromGoDuration((string)serializer.Deserialize(reader, typeof(string)));
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeSpan))
            {
                return true;
            }
            return false;
        }
    }

    public class DurationTimespanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((TimeSpan)value).ToGoDuration());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return Extensions.FromGoDuration((string)serializer.Deserialize(reader, typeof(string)));
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeSpan))
            {
                return true;
            }
            return false;
        }
    }

    public class KVPairConverter : JsonConverter
    {
        static Lazy<string[]> objProps = new Lazy<string[]>(() => typeof(KVPair).GetRuntimeProperties().Select(p => p.Name).ToArray());

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var result = new KVPair();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject) { continue; }
                if (reader.TokenType == JsonToken.EndObject) { return result; }
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var jsonPropName = reader.Value.ToString();
                    var propName = objProps.Value.FirstOrDefault(p => p.Equals(jsonPropName, StringComparison.OrdinalIgnoreCase));
                    if (propName != null)
                    {
                        var pi = result.GetType().GetRuntimeProperty(propName);

                        if (jsonPropName.Equals("Flags", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(reader.ReadAsString()))
                            {
                                var val = Convert.ToUInt64(reader.Value);
                                pi.SetValue(result, val, null);
                            }
                        }
                        else if (jsonPropName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(reader.ReadAsString()))
                            {
                                var val = Convert.FromBase64String(reader.Value.ToString());
                                pi.SetValue(result, val, null);
                            }
                        }
                        else
                        {
                            if (reader.Read())
                            {
                                var convertedValue = Convert.ChangeType(reader.Value, pi.PropertyType);
                                pi.SetValue(result, convertedValue, null);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(KVPair))
            {
                return true;
            }
            return false;
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }
}