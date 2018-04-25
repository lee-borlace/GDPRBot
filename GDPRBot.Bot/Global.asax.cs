using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace GDPRBot.Bot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var builder = new ContainerBuilder();

            builder.RegisterModule(new DialogModule());

            builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

            builder.Register(c => new TableBotDataStore(ConfigurationManager.ConnectionStrings["MS_AzureStoreConnectionString"].ConnectionString))
                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                .AsSelf()
                .SingleInstance();

            builder.Update(Conversation.Container);
        }
    }
}
