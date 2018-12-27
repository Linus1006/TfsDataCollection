using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ESUN.Framework
{
    public static class SerializeHelper
    {
        #region Json
        private static JsonSerializer CreateJsonSerializer(bool? withType)
        {
            var serializer = new JsonSerializer();
            if (withType == true) serializer.TypeNameHandling = TypeNameHandling.All;
            return serializer;
        }
        private static T FromJson<T>(TextReader textReader, bool? withType)
        {
            using (var reader = new JsonTextReader(textReader) { CloseInput = false })
            {
                return CreateJsonSerializer(withType).Deserialize<T>(reader);
            }
        }

        public static T FromJson<T>(Stream stream, bool? withType = null)
        {
            //用這個建構式只為了leaveOpen = true
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return FromJson<T>(reader, withType);
            }
        }

        public static T FromJson<T>(string text, bool? withType = null)
        {
            if (text == null) return default(T);
            using (var reader = new StringReader(text))
            {
                return FromJson<T>(reader, withType);
            }
        }

        public static void ToJson(Stream stream, object value, bool? withType = null)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    CreateJsonSerializer(withType).Serialize(jsonWriter, value);
                }
            }
        }

        private static void ToJson(TextWriter textWriter, object value, bool? withType)
        {
            using (var writer = new JsonTextWriter(textWriter))
            {
                CreateJsonSerializer(withType).Serialize(writer, value);
            }
        }

        public static string ToJson(object value, bool? withType = null)
        {
            if (value == null) return null;
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                ToJson(writer, value, withType);
            }
            return sb.ToString();
        }
        #endregion

        #region Binary
        private static BinaryFormatter CreateBinaryFormatter()
        {
            return new BinaryFormatter();
        }
        public static T FromBinary<T>(Stream stream)
        {
            return (T)CreateBinaryFormatter().Deserialize(stream);
        }

        public static T FromBinary<T>(byte[] data)
        {
            using(var stream = new MemoryStream(data))
            {
                return FromBinary<T>(stream);
            }
        }

        public static void ToBinary(Stream stream, object value)
        {
            CreateBinaryFormatter().Serialize(stream, value);
        }

        public static byte[] ToBinary(object value)
        {
            using(var stream = new MemoryStream())
            {
                ToBinary(stream, value);
                return stream.ToArray();
            }
        }
        #endregion 

        public static T Clone<T>(T source)
        {
            if (Object.ReferenceEquals(source, null)) return default(T);

            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            IFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// 深層複製(需使用Json.Net組件)
        /// </summary>
        /// <typeparam name="T">複製對象類別</typeparam>
        /// <param name="source">複製對象</param>
        /// <returns>複製品</returns>
        public static T DeepCloneViaJson<T>(this T source)
        {

            if (source != null)
            {
                // avoid self reference loop issue
                // track object references when serializing and deserializing JSON
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                var serializedObj = JsonConvert.SerializeObject(source, Formatting.Indented, jsonSerializerSettings);
                return JsonConvert.DeserializeObject<T>(serializedObj, jsonSerializerSettings);
            }
            else
            { return default(T); }

        }
    }
}
