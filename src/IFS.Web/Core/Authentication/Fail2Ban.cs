using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IFS.Web.Core.Authentication
{
    public interface IFail2Ban
    {
        void RecordFailure(HttpContext httpContext);
        void RecordSuccess(HttpContext httpContext);
        bool IsRateLimitApplied(HttpContext httpContext);
    }

    public class Fail2Ban : IFail2Ban
    {
        private readonly ConcurrentDictionary<string, Fail2BanRecord> _recordStore;

        private readonly Fail2BanOptions _options;
        private readonly ILogger<Fail2Ban> _logger;

        public Fail2Ban(IOptions<Fail2BanOptions> options, ILoggerFactory loggerFactory)
        {
            this._options = options.Value;
            this._logger = loggerFactory.CreateLogger<Fail2Ban>();
            this._recordStore = new ConcurrentDictionary<string, Fail2BanRecord>();
        }

        public void RecordFailure(HttpContext httpContext)
        {
            string requesterIdentifier = GetUniqueRequesterIdentifier(httpContext);

            LogMessages.OnFailure(this._logger, requesterIdentifier, null);

            Fail2BanRecord newRecord = this._recordStore.AddOrUpdate(
                requesterIdentifier,
                _ => Fail2BanRecord.Create(),
                (_, record) => record.Record()
            );

            if (CheckApplyRateLimiting(newRecord) == true)
            {
                LogMessages.OnRateLimitingApplied(this._logger, requesterIdentifier, null);
            }
        }

        public void RecordSuccess(HttpContext httpContext)
        {
            string requesterIdentifier = GetUniqueRequesterIdentifier(httpContext);

            bool success = this._recordStore.TryRemove(requesterIdentifier, out _);

            if (success)
            {
                LogMessages.OnSuccess(this._logger, requesterIdentifier, null);
            }
        }

        public bool IsRateLimitApplied(HttpContext httpContext)
        {
            string requesterIdentifier = GetUniqueRequesterIdentifier(httpContext);

            if (this._recordStore.TryGetValue(requesterIdentifier, out Fail2BanRecord record))
            {
                // Yes there is a potential race condition here in which a failure is recorded
                // while the record expires. I don't care, it is not bad enough to worry about.
                bool? applyRateLimiting = CheckApplyRateLimiting(record);

                if (applyRateLimiting == false)
                {
                    bool success = this._recordStore.TryRemove(requesterIdentifier, out _);

                    if (success)
                    {
                        LogMessages.OnRateLimitingRelaxed(this._logger, requesterIdentifier, null);
                    }
                }

                return applyRateLimiting == true;
            }

            // We didn't record, so its all good
            return false;
        }

        private bool? CheckApplyRateLimiting(Fail2BanRecord record)
        {
            TimeSpan timeDiff = record.MostRecentFailure - record.FirstFailure;
            
            // If this record is stale, kick it out
            if (timeDiff > this._options.DebounceTime)
            {
                return false;
            }

            // If we haven't reached the count yet, hold on to the record
            if (record.FailureCount <= this._options.MaximumAttempts)
            {
                return null;
            }

            return true;
        }

        private static string GetUniqueRequesterIdentifier(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }

        private sealed class Fail2BanRecord
        {
            public DateTimeOffset FirstFailure { get; } = DateTimeOffset.UtcNow;
            public DateTimeOffset MostRecentFailure { get; } = DateTimeOffset.UtcNow;

            public int FailureCount { get; } = 1;

            private Fail2BanRecord(Fail2BanRecord copy)
            {
                this.FirstFailure = copy.FirstFailure;
                this.FailureCount = copy.FailureCount + 1;
            }

            private Fail2BanRecord() { }

            public Fail2BanRecord Record()
            {
                return new Fail2BanRecord(this);
            }

            public static Fail2BanRecord Create() => new Fail2BanRecord();
        }

        private static class LogMessages
        {
            public static Action<ILogger, string, Exception> OnFailure = LoggerMessage.Define<string>(
                LogLevel.Information,
                default,
                "Requester {0} was marked for failure."
            );

            public static Action<ILogger, string, Exception> OnSuccess = LoggerMessage.Define<string>(
                LogLevel.Information,
                default,
                "Requester {0} was marked clean."
            );

            public static Action<ILogger, string, Exception> OnRateLimitingApplied = LoggerMessage.Define<string>(
                LogLevel.Information,
                default,
                "Requester {0} now has rate-limiting applied."
            );

            public static Action<ILogger, string, Exception> OnRateLimitingRelaxed = LoggerMessage.Define<string>(
                LogLevel.Information,
                default,
                "Requester {0} now has rate-limiting relaxed."
            );
        }
    }
}