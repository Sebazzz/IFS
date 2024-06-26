﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using IFS.Web.Core.Crypto;
using Newtonsoft.Json;

namespace IFS.Web.Core.Upload;

public class StoredMetadata {
    private AccessLog? _access;

    public DateTime Expiration { get; set; }
    public DateTime UploadedOn { get; set; }

    public bool IsReservation { get; set; }
    public AccessLog Access {
        get => this._access ??= new AccessLog();
        set => this._access = value;
    }

    public string? OriginalFileName { get; set; }

    public ContactInformation? Sender { get; set; }

    public DownloadSecurity? DownloadSecurity { get; set; }
    public long OriginalFileSizeBytes { get; set; }

    public string Serialize() {
        return JsonConvert.SerializeObject(this, Formatting.Indented, SerializerSettings);
    }

    public static StoredMetadata Deserialize(string str) {
        return JsonConvert.DeserializeObject<StoredMetadata>(str, SerializerSettings);
    }

    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Ignore
    };
}

public class DownloadSecurity {
    public string? SecurityToken { get; set; }
    public string? HashedPassword { get; set; }

    public static DownloadSecurity CreateNew(string password) {
        string securityToken = Guid.NewGuid().ToString("n");
        return new DownloadSecurity {
            SecurityToken = securityToken,
            HashedPassword = PasswordHasher.HashPassword(password, securityToken)
        };
    }

    public bool Verify([NotNullWhen(true)]string? password) {
        string? hashedPassword = password != null ? PasswordHasher.HashPassword(password, this.SecurityToken!) : null;
        return hashedPassword == this.HashedPassword;
    }
}

public class ContactInformation {
    [Display(Name = "Name")]
    [StringLength(256)]
    public string? Name { get; set; }
    [Display(Name = "E-mail address")]
    [DataType(DataType.EmailAddress)]
    [StringLength(256)]
    public string? EmailAddress { get; set; }
}

public class AccessLog {
    private List<FileAccessLogEntry>? _logEntries;

    public List<FileAccessLogEntry> LogEntries {
        get => this._logEntries ??= new List<FileAccessLogEntry>();
        set => this._logEntries = value;
    }
}

public class FileAccessLogEntry {
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}