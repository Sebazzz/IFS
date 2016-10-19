namespace IFS.Web.Core.Upload {
    using System;

    using Newtonsoft.Json;

    public class StoredMetadata {
        public DateTime Expiration { get; set; }
        public string OriginalFileName { get; set; }

        public string Serialize() {
            return JsonConvert.SerializeObject(this, Formatting.Indented, SerializerSettings);
        }

        public static StoredMetadata Deserialize(string str) {
            return JsonConvert.DeserializeObject<StoredMetadata>(str, SerializerSettings);
        }

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();
    }
}