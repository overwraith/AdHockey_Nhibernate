using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AdHockey.Startup))]
namespace AdHockey
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
