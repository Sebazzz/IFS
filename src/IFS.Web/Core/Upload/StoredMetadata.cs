namespace IFS.Web.Core.Upload {
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class StoredMetadata {
        public DateTime Expiration { get; set; }
        public DateTime UploadedOn { get; set; }

        public bool IsReservation { get; set; }
        public AccessLog Access {
            get => this._access ?? (this._access = new AccessLog());
            set => this._access = value;
        }

        public string OriginalFileName { get; set; }

        public string Serialize() {
            return JsonConvert.SerializeObject(this, Formatting.Indented, SerializerSettings);
        }

        public static StoredMetadata Deserialize(string str) {
            return JsonConvert.DeserializeObject<StoredMetadata>(str, SerializerSettings);
        }

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();
        private AccessLog _access;
    }

    public class AccessLog {
        private List<FileAccessLogEntry> _logEntries;

        public List<FileAccessLogEntry> LogEntries {
            get => this._logEntries ?? (this._logEntries = new List<FileAccessLogEntry>());
            set => this._logEntries = value;
        }
    }

    public class FileAccessLogEntry {
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }
}