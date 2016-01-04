using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DapperIdentity.Web.Startup))]
namespace DapperIdentity.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
