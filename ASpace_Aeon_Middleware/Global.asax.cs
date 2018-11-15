using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using ArchivesSpace_.Net_Client.Models;
using ASpace_Aeon_Middleware.Interfaces;
using ASpace_Aeon_Middleware.Models;
using Autofac;
using Autofac.Integration.WebApi;
using WebApiContrib.Formatting;

namespace ASpace_Aeon_Middleware
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var config = GlobalConfiguration.Configuration;
            
            //Set up dependency injection
            var builder = BuildDependencies();
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            //Custom formatter
            config.Formatters.Add(new PlainTextFormatter());
            
            //Configure custom serialization
            var seriesListSerializer = new DataContractSerializer(typeof(List<SeriesResponseItem>), "rows", String.Empty); //the js expects a root node of "rows"
            var enumListSerializer = new DataContractSerializer(typeof(List<EnumResponseItem>), "rows", String.Empty);
            var barcodeListSerializer = new DataContractSerializer(typeof(List<BarcodeResponseItem>), "rows", String.Empty);
            var combinedBarcodeListSerializer = new DataContractSerializer(typeof(List<CombinedBarcodeResponseItem>), "rows", String.Empty); 
            config.Formatters.XmlFormatter.SetSerializer(typeof(List<SeriesResponseItem>), seriesListSerializer);
            config.Formatters.XmlFormatter.SetSerializer(typeof(List<EnumResponseItem>), enumListSerializer);
            config.Formatters.XmlFormatter.SetSerializer(typeof(List<BarcodeResponseItem>), barcodeListSerializer);
            config.Formatters.XmlFormatter.SetSerializer(typeof(List<CombinedBarcodeResponseItem>), combinedBarcodeListSerializer);

            //Register the WebAPI framework
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
        }

        protected ContainerBuilder BuildDependencies()
        {
            var aspaceContainer = ConfigureAspace();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(aspaceContainer).As<IAspaceSiteServiceHandler>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            return builder;
        }

        private AspaceSiteMultiServiceHandler ConfigureAspace()
        {
            var serviceHandler = new AspaceSiteMultiServiceHandler(GetAspaceCredential());
            return serviceHandler;
        }

        private ArchivesSpaceCredential GetAspaceCredential()
        {
            var cred = new ArchivesSpaceCredential
            {
                Username = Configuration.ArchivesSpaceUsername,
                Password = Configuration.ArchivesSpacePassword
            };
            return cred;
        }
    }
}
