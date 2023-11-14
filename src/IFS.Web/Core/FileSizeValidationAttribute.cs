// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileSizeValidationAttribute.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.ComponentModel.DataAnnotations;
using Humanizer;
using IFS.Web.Core.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IFS.Web.Core;

public sealed class FileSizeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (!(value is IFormFile file)) return new ValidationResult("File is required");

        var options = validationContext.GetRequiredService<IOptions<FileStoreOptions>>().Value;

        var maxFileSizeInBytes = (long)options.MaximumFileSize.Megabytes().Bytes;
        if (file.Length > maxFileSizeInBytes)
        {
            var actualSize = file.Length.Bytes().Humanize("MB");
            var maximumSize = options.MaximumFileSize.Megabytes().ToString("MB");

            return new ValidationResult(
                $"The file is too large, it is {actualSize} but the maximum allowed size {maximumSize}");
        }

        return ValidationResult.Success;
    }
}