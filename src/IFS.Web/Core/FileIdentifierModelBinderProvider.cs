// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileIdentifierModelBinderProvider.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core {
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    using Models;

    internal sealed class FileIdentifierModelBinderProvider : IModelBinderProvider {
        public IModelBinder GetBinder(ModelBinderProviderContext context) {
            if (context.Metadata.ModelType == typeof(FileIdentifier)) {
                return new ModelBinder();
            }

            return null!;
        }

        private sealed class ModelBinder : IModelBinder {
            public Task BindModelAsync(ModelBindingContext bindingContext) {
                ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

                string stringValue = value.FirstValue;
                if (String.IsNullOrEmpty(stringValue)) {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Empty file identifier");
                } else {
                    try {
                        FileIdentifier id = FileIdentifier.FromString(stringValue);

                        bindingContext.Model = id;
                        bindingContext.Result = ModelBindingResult.Success(id);
                    }
                    catch (ArgumentException ex) {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
