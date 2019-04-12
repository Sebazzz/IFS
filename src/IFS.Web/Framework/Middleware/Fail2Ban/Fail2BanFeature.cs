// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Fail2BanFeature.cs
//  Project         : IFS.Web
// ******************************************************************************
namespace IFS.Web.Framework.Middleware.Fail2Ban
{
    public sealed class Fail2BanFeature
    {
        public bool IsSuccess { get; }

        public Fail2BanFeature(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }
}