using Autofac;
using AzureFunctions.Autofac.Configuration;
using Common;

namespace SampleDurableFunction
{
    public class DIConfig
    {
        public DIConfig(string functionName)
        {
            DependencyInjection.Initialize(x => x.RegisterType<UserAgeValidator>().As<IUserAgeValidator>(), functionName);
        }
    }
}
