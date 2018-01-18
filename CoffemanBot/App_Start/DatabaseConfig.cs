using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace CoffemanBot
{
    public static class DatabaseConfig
    {
        public static void Register(HttpConfiguration config)
        {
#if DEBUG
            Conversation.UpdateContainer(
            builder =>
            {
                var store = new InMemoryDataStore();
                builder.Register(c => store)
                          .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                          .AsSelf()
                          .SingleInstance();
            });
#else
            Conversation.UpdateContainer(
                    builder =>
                    {
                        //builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));
                        var appKey = ConfigurationManager.ConnectionStrings["StorageConnectionString"];
                        var store = new TableBotDataStore(appKey.ConnectionString);
                        builder.Register(c => store)
                            .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                            .AsSelf()
                            .SingleInstance();

                        // Register your Web API controllers.
                        // builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                        // builder.RegisterWebApiFilterProvider(config);

                    });

            //config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);
#endif
        }
    }
}