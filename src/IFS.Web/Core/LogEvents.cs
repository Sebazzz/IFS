// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : LogEvents.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.Extensions.Logging;

namespace IFS.Web.Core;

public static class LogEvents
{
    public static readonly EventId NewUpload = new(0001, nameof(NewUpload));
    public static readonly EventId UploadExpired = new(0002, nameof(UploadExpired));

    public static readonly EventId UploadFailed = new(1000, nameof(UploadFailed));
    public static readonly EventId UploadCancelled = new(2000, nameof(UploadFailed));
    public static readonly EventId CleanupFailed = new(1001, nameof(CleanupFailed));

    public static readonly EventId UploadNotFound = new(2001, nameof(UploadNotFound));
    public static readonly EventId UploadIncomplete = new(1002, nameof(UploadIncomplete));
    public static readonly EventId UploadReservationTaken = new(1003, nameof(UploadReservationTaken));

    public static readonly EventId UploadCorrupted = new(0xDEAD, nameof(UploadCorrupted));

    public static readonly EventId UploadPasswordAfterFileUpload = new(0xC000, nameof(UploadPasswordAfterFileUpload));
}
