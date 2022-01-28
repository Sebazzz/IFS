// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AutoFillSenderInformation.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Core.Upload;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IFS.Web.Models;

namespace IFS.Web.Framework.Filters {
    public sealed class AutoFillSenderInformationAttribute : ActionFilterAttribute {
        public override void OnActionExecuted(ActionExecutedContext context) {
            // Note we execute this on action end, the user can tamper with the HTML and override it.
            // It's not that important to take more measures.

            if (context.Result is ViewResult vr && vr.Model is IContactInformationModel contactInformationModel) {
                SetContactInformation(contactInformationModel, context.HttpContext.User);
            }
        }

        private static void SetContactInformation(IContactInformationModel contactInformationModel, ClaimsPrincipal user) {
            string displayName = user.FindFirstValue(ClaimTypes.GivenName);
            string email = user.FindFirstValue(ClaimTypes.Email);

            contactInformationModel.Sender ??= new ContactInformation();
            contactInformationModel.Sender.Name = displayName;
            contactInformationModel.Sender.EmailAddress = email;

            if (!String.IsNullOrEmpty(contactInformationModel.Sender.Name) && !String.IsNullOrEmpty(contactInformationModel.Sender.EmailAddress)) {
                contactInformationModel.IsSenderInformationPrefilled = true;
            }
        }
    }
}