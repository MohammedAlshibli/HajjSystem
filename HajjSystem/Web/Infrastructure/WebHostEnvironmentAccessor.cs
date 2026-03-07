using HajjSystem.Infrastructure.Services;

namespace HajjSystem.Web.Infrastructure;

public class WebHostEnvironmentAccessor : IWebHostEnvironmentAccessor
{
    public string ContentRootPath { get; }
    public WebHostEnvironmentAccessor(IWebHostEnvironment env)
        => ContentRootPath = env.ContentRootPath;
}
