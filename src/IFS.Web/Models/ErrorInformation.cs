﻿// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ErrorInformation.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using Microsoft.AspNetCore.Http;

namespace IFS.Web.Models;

public class ErrorInformation
{
    public string? OriginalPath { get; set; }

    public string? QueryString { get; set; }

    public string? GetUrl(HttpRequest request)
    {
        if (string.IsNullOrEmpty(this.OriginalPath)) return null;

        var uriBuilder = new UriBuilder
        {
            Scheme = request.Scheme,
            Host = request.Host.Host,
            Path = this.OriginalPath,
            Query = this.QueryString ?? string.Empty
        };

        return uriBuilder.ToString();
    }
}