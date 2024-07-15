using DevIO.Api.Controllers.Base;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers.v2;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TestController : MainController
{
    private readonly ILogger _logger;

    public TestController(INotifier notifier, IUser appUser, ILogger<TestController> logger) : base(notifier, appUser)
    {
        _logger = logger;
    }

    [HttpGet]
    public string Value()
    {
        throw new Exception("Erro");

        //try
        //{
        //    var i = 0;
        //    var result = 42 / i;
        //}
        //catch (DivideByZeroException e)
        //{
        //    e.Ship(HttpContext);
        //}

        _logger.LogTrace("Log de Trace");
        _logger.LogDebug("Log de Debug");
        _logger.LogInformation("Log de Informação");
        _logger.LogWarning("Log de Aviso");
        _logger.LogError("Log de Erro");
        _logger.LogCritical("Log de Problema Crítico");

        return "I'm the v2";
    }
}
