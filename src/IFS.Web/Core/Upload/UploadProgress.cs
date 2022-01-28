using System;

namespace IFS.Web.Core.Upload {
    public class UploadProgress {
        public long Current { get; set; }
        public long Total { get; set; }

        public string? FileName { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
    }
}