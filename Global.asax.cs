using ProcurementSystem.Data; 
using ProcurementSystem.Models; 
using System;
using System.Data.Entity; 
using System.Linq;
using System.Security.Principal; 
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security; 

namespace ProcurementSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new DbInitializer());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
            {
                return;
            }

            FormsAuthenticationTicket authTicket;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            if (authTicket == null || authTicket.Expired)
            {
                return;
            }

            var login = authTicket.Name;
            if (string.IsNullOrEmpty(login))
            {
                return;
            }

            using (var db = new ProcurementContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);

                if (user != null)
                {
                    string role = user.Role.ToString(); 
                    var principal = new GenericPrincipal(new GenericIdentity(login), new[] { role });
                    Context.User = principal;
                }
            }
        }
    }
}