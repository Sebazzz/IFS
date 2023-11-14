// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileIdentifierModelBinderProvider.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Threading.Tasks;
using IFS.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IFS.Web.Core;

internal sealed class FileIdentifierModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(FileIdentifier)) return new ModelBinder();

        return null!;
    }

    private sealed class ModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var stringValue = value.FirstValue;
            if (string.IsNullOrEmpty(stringValue))
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Empty file identifier");
            else
                try
                {
                    var id = FileIdentifier.FromString(stringValue);

                    bindingContext.Model = id;
                    bindingContext.Result = ModelBindingResult.Success(id);
                }
                catch (ArgumentException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                }

            return Task.CompletedTask;
        }
    }
}