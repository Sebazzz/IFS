// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Fail2BanOptions.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;

namespace IFS.Web.Core.Authentication;

public class Fail2BanOptions
{
    public TimeSpan DebounceTime { get; set; } = TimeSpan.FromMinutes(10);

    public int MaximumAttempts { get; set; } = 10;
}