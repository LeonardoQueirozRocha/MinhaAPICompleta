using DevIO.Api.Configurations;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Services.AddAppSettingsConfiguration(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityConfiguration(builder.Configuration, appSettings.AuthConfiguration);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddApiConfiguration();

builder.Services.AddSwaggerConfiguration();

builder.Services.AddLoggingConfiguration(builder.Configuration, appSettings.LogConfiguration);

builder.Services.AddDependencyInjectionConfiguration();

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseApiConfiguration(app.Environment);

app.UseSwaggerConfiguration(provider);

app.UseLoggingConfiguration();

app.Run();