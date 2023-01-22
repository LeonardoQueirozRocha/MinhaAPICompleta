using DevIO.Api.Controllers;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.V2.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestController : MainController
    {
        public TestController(INotifier notifier, IUser appUser) : base(notifier, appUser)
        {
        }

        [HttpGet]
        public string Value()
        {
            return "I'm the v2";
        }
    }
}
