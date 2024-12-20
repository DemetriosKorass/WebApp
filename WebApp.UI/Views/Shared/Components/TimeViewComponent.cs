using Microsoft.AspNetCore.Mvc;
using WebApp.UI.Services;

public class TimeViewComponent : ViewComponent
{
    private readonly DateTimeService _dateTimeService;

    public TimeViewComponent(DateTimeService dateTimeService)
    {
        _dateTimeService = dateTimeService;
    }

    public IViewComponentResult Invoke()
    {
        var currentTime = _dateTimeService.GetCurrentUtcTime();
        return View("Default", currentTime);
    }
}
