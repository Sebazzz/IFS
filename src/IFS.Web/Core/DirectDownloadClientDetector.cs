// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DirectDownloadClientDetector.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Linq;

namespace IFS.Web.Core {
    public static class DirectDownloadClientDetector {
        public static bool IsDirectDownloadClient(string userAgent) {
            string[] patterns = {"WGet", "Curl", "WebClient"};

            return String.IsNullOrEmpty(userAgent) || patterns.Any(pattern => userAgent.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) != -1);
        }
    }
}
