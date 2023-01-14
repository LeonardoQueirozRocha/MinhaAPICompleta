using DevIO.Api.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevIO.Api.Extensions
{
    // Binder personalizado para envio de IFormFile e ViewModel dentro de um FormData compatível com .NET Core 3.1 ou superior (system.text.json)
    public class ProdutoModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            var produtoImagemDto = JsonSerializer.Deserialize<ProdutoImagemDto>(bindingContext.ValueProvider.GetValue("produto").FirstOrDefault(), serializeOptions);
            produtoImagemDto.ImagemUpload = bindingContext.ActionContext.HttpContext.Request.Form.Files.FirstOrDefault();

            bindingContext.Result = ModelBindingResult.Success(produtoImagemDto);
            return Task.CompletedTask;
        }
    }
}
