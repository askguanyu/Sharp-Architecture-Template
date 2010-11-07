namespace SharpArchTemplate.Web
{
    using System;
    using System.Configuration;
    using System.Web;

    using Castle.Windsor;
    using Castle.Windsor.Configuration.Interpreters;

    using CommonServiceLocator.WindsorAdapter;

    using Microsoft.Practices.ServiceLocation;

    using SharpArch.Data.NHibernate;
    using SharpArch.Web.NHibernate;

    using SharpArchTemplate.Infrastructure.NHibernateMaps;

    /// <summary>
    ///   Represents the MVC Application
    /// </summary>
    /// <remarks>
    ///   For instructions on enabling IIS6 or IIS7 classic mode, 
    ///   visit http://go.microsoft.com/?LinkId=9394801
    /// </remarks>
    public class MvcApplication : HttpApplication
    {
        private static bool showCustomErrorPages;

        private WebSessionStorage webSessionStorage;

        /// <summary>
        ///   Due to issues on IIS7, the NHibernate initialization must occur in Init().
        ///   But Init() may be invoked more than once; accordingly, we introduce a thread-safe
        ///   mechanism to ensure it's only initialized once.
        ///   See http://msdn.microsoft.com/en-us/magazine/cc188793.aspx for explanation details.
        /// </summary>
        public override void Init()
        {
            this.webSessionStorage = new WebSessionStorage(this);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Only initialise the NHibernate Sessions for a non static, i.e. Controller Action request
            NHibernateInitializer.Instance().InitializeNHibernateOnce(this.InitialiseNHibernateSessions);
        }

        protected void Application_Start()
        {
            var container = InitializeServiceLocator();
        }

        private static IWindsorContainer InitializeServiceLocator()
        {
            IWindsorContainer container = new WindsorContainer(new XmlInterpreter());

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            return container;
        }

        private void InitialiseNHibernateSessions()
        {
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache();

            NHibernateSession.Init(
                this.webSessionStorage, 
                new[] { this.Server.MapPath("~/bin/SharpArchTemplate.Infrastructure.dll") }, 
                new AutoPersistenceModelGenerator().Generate(), 
                this.Server.MapPath("~/Configuration/NHibernate/nhibernate.current_session.config"));
        }
    }
}