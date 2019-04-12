// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ServiceCollectionExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using IFS.Web.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IFS.Web.Framework
{
    internal static class ServiceCollectionExtensions
    {
        public static IFail2Ban GetFail2Ban(this IServiceProvider services) => services.GetRequiredService<IFail2Ban>();
    }
}