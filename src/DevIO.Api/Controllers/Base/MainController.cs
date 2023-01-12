using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIO.Api.Controllers.Base
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotifier _notifier;

        public MainController(INotifier notifier)
        {
            _notifier = notifier;
        }

        protected bool IsValid()
        {
            return !_notifier.HaveNotification();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (IsValid()) return Ok(new { success = true, data = result });

            var errors = _notifier.GetNotifications().Select(notification => notification.Message);

            return BadRequest(new { success = false, errors });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) ReportInvalidModelError(modelState);

            return CustomResponse();
        }

        protected void ReportInvalidModelError(ModelStateDictionary modelState)
        {
            var errors = modelState.Values.SelectMany(value => value.Errors);

            foreach (var error in errors)
            {
                var errorMessage = error.Exception == null ? error.ErrorMessage : error.Exception.Message;
                ReportError(errorMessage);
            }
        }

        protected void ReportError(string message)
        {
            _notifier.Handle(new Notification(message));
        }
    }
}