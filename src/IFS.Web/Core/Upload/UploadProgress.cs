namespace IFS.Web.Core.Upload {
    using System;

    public class UploadProgress {
        public long Current { get; set; }
        public long Total { get; set; }

        public string? FileName { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
    }
}