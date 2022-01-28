// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : IContactInformationModel.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Core.Upload;

namespace IFS.Web.Models;

public interface IContactInformationModel {
    ContactInformation? Sender { get; set; }
    bool IsSenderInformationPrefilled { get; set; }
}