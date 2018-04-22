using ICalTimeZoneOverrideService.Middleware;
using ICalTimeZoneOverrideService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.Builder
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseIcalTimeTravel(
            this IApplicationBuilder builder,
            TimeTravelOptions options)
        {

            builder.UseMiddleware<IcalTimeTravelMiddleware>(options);
            return builder;
        }
    }
}


namespace ICalTimeZoneOverrideService.Middleware
{


    public class TimeTravelOptions
    {
        public string Path { get; set; } = "/";
    }


    public class IcalTimeTravelMiddleware
    {
        private readonly RequestDelegate next;
        private readonly TimeTravelOptions options;
        private readonly ILogger logger;

        public IcalTimeTravelMiddleware(RequestDelegate next, TimeTravelOptions options,
                                  ILoggerFactory logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger.CreateLogger<IcalTimeTravelMiddleware>();
        }

        public async Task Invoke(HttpContext context, IIcalTimeTravelService greeter)
        {
            if (context.Request.Path.StartsWithSegments(options.Path))
            {
                logger.LogInformation("IcalTimeTravelMiddleware middleware handling request");

                var options = HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
                if ((options["tzid"] != null) && options["url"] != null)
                {
                    var tzid = options["tzid"];
                    var url = options["url"];


                    HttpClient httpClient = new HttpClient();
                    try
                    {
                        var response = httpClient.GetAsync(url).Result;
                        string body = response.Content.ReadAsStringAsync().Result;

                        //We could have gone through ical, but string parsing is just fun...
                        //var calendar = Ical.Net.Calendar.Load(body);

                        using (StringWriter output = new StringWriter())
                        using (StringReader reader = new StringReader(body))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.StartsWith("DTEND:") || line.StartsWith("DTSTART:"))
                                {
                                    if (!line.EndsWith("Z"))
                                    {
                                        // Local time missing local timezone reference
                                        string tz = ";TZID="+ tzid +":";
                                        if (line.StartsWith("DTSTART:"))
                                        {
                                            line = line.Replace("DTSTART:", "DTSTART" + tz);
                                        }
                                        if (line.StartsWith("DTEND:"))
                                        {
                                            line = line.Replace("DTEND:", "DTEND" + tz);
                                        }
                                    }
                                }
                                output.WriteLine(line);
                            }

                            context.Response.ContentType = "text/plain";
                            await context.Response.WriteAsync($"TzId:{tzid}\nUrl:{url}\n\n{output.ToString()}");
                            return;

                            context.Response.ContentType = "application/octet-stream";
                            await context.Response.WriteAsync(output.ToString());
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync($"TzId:{tzid}\nUrl:{url}\n\nError: {ex.ToString()}");
                        throw;
                    }
                    return;
                }

                var message = greeter.GetWelcomeMessage();
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync($"{message}\nPlease provide the querystring parameters ?tzid= &url=\nE.g.h/?tzid=Europe/Copenhagen&url=http://ical.sport-solution.com/1908586.ics");
            }
            else
            {
                logger.LogInformation($"IcalTimeTravelMiddleware allowing {context.Request.Path} to pass through");
                await next(context);
                // ...
                logger.LogInformation($"Previous middlware complete with {context.Response.StatusCode}");

            }
        }

    }
}