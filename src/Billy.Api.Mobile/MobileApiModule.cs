using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Billy.Api.Mobile.ExpensesService;
using Flurl.Http.Configuration;

namespace Billy.Api.Mobile
{
    public class MobileApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReceiptsServiceClient>().SingleInstance();
            builder.RegisterType<ExpensesServiceClient>().SingleInstance();
            builder.RegisterType<PerBaseUrlFlurlClientFactory>().As<IFlurlClientFactory>();
        }
    }
}
