using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ICalTimeZoneOverrideService.Services
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddIcalTimeTravel(this IServiceCollection services)
        {
            services.AddSingleton<IIcalTimeTravelService, IcalTimeTravelService>();
            return services;
        }
    }


    public interface IIcalTimeTravelService
    {
        string GetWelcomeMessage();
    }


    public class IcalTimeTravelService : IIcalTimeTravelService
    {
        private readonly IConfiguration config;

        public IcalTimeTravelService(IConfiguration config)
        {
            this.config = config;
        }

        public string GetWelcomeMessage()
        {
            return config["WelcomeMessage"];
        }
    }
}
