using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ProcurementSystem.Data;
using System.Web.Security;
using System.Security.Principal;

namespace ProcurementSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new DbInitializer());

            // Принудительно инициализируем контекст при старте — это создаст/заполняет БД по DbInitializer,
            // если она ещё не создана (полезно в разработке).
            using (var ctx = new ProcurementContext())
            {
                ctx.Database.Initialize(force: true);
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
            {
                return;
            }

            try
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null) return;

                var roles = string.IsNullOrEmpty(ticket.UserData)
                    ? new string[] { }
                    : ticket.UserData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                var identity = new GenericIdentity(ticket.Name);
                HttpContext.Current.User = new GenericPrincipal(identity, roles);
            }
            catch
            {
            }
        }
    }
}