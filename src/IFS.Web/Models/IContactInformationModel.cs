// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : IContactInformationModel.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Models {
    using Core.Upload;

    public interface IContactInformationModel {
        ContactInformation Sender { get; set; }
        bool IsSenderInformationPrefilled { get; set; }
    }
}