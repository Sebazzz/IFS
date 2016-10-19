// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : IUploadManager.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using Models;

    public interface IUploadManager {
        UploadProgress GetProgress(FileIdentifier id);
        Task StoreAsync(FileIdentifier id, IFormFile file, CancellationToken cancellationToken);
    }
}
