// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadModelFactory.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.ModelFactory {
    using System;
    using System.Globalization;

    using Humanizer;

    using Microsoft.AspNetCore.Mvc.Rendering;

    using Models;

    internal static class UploadModelFactory {
        private static TModel Create<TModel>() where TModel : UploadModelBase,new() {
            SelectListItem CreateItem(TimeSpan timespan) => new SelectListItem {
                Value = (DateTime.UtcNow + timespan).ToString("O"),
                Text = timespan.Humanize()
            };

            SelectListItem CreateMonthItem(int month) => new SelectListItem {
                Value = CultureInfo.CurrentCulture.Calendar.AddMonths(DateTime.UtcNow, month).ToString("O"),
                Text = $"{month} months"
            };

            TModel uploadModel = new TModel {
                FileIdentifier = FileIdentifier.CreateNew(),
                Expiration = DateTime.UtcNow.AddDays(7),
                AvailableExpiration = new[] {
                    CreateItem(TimeSpan.FromHours(1)),
                    CreateItem(TimeSpan.FromHours(4)),
                    CreateItem(TimeSpan.FromHours(8)),
                    CreateItem(TimeSpan.FromDays(1)),
                    CreateItem(TimeSpan.FromDays(2)),
                    CreateItem(TimeSpan.FromDays(7)),
                    CreateMonthItem(1),
                    CreateMonthItem(2),
                    CreateMonthItem(3),
                    CreateMonthItem(6),
                    CreateItem(TimeSpan.FromDays(CultureInfo.CurrentCulture.Calendar.GetDaysInYear(DateTime.UtcNow.Year))),
                }
            };

            return uploadModel;
        }

        public static UploadModel Create() {
            return Create<UploadModel>();
        }

        public static UploadLinkModel CreateLink() {
            return Create<UploadLinkModel>();
        }
    }
}
