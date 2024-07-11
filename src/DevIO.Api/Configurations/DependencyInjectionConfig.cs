using DevIO.Api.Extensions.Authorization;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Interfaces.User;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using DevIO.Data.Context;
using DevIO.Data.Repository;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DevIO.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddScoped<AppDbContext>();
            services.AddScoped<IFornecedorRepository, FornecedorRepository>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            services.AddScoped<IProdutoRepository, ProdutoRepository>();

            services.AddScoped<INotifier, Notifier>();
            services.AddScoped<IFornecedorService, FornecedorService>();
            services.AddScoped<IEnderecoService, EnderecoService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IProdutoService, ProdutoService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, AspNetUser>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }
    }
}
