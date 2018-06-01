using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ai.pdm.bot.models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest.TransientFaultHandling;

namespace ai.pdm.bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IAccountsRepository>(new AccountsRepository());
            services.AddBot<EchoBot>(options =>
            {
                var middleware = options.Middleware;

                //                middleware.Add(new UserState<UserData>(new MemoryStorage()));
                //                middleware.Add(new ConversationState<ConversationData>(new MemoryStorage()));
                middleware.Add(new RegExpRecognizerMiddleware()
                                .AddIntent("mystarts", new Regex("starts|top", RegexOptions.IgnoreCase))
                                .AddIntent("howtohelp", new Regex("help (?<partner>.*)", RegexOptions.IgnoreCase))
                                .AddIntent("myworries", new Regex("worried|worry|worries", RegexOptions.IgnoreCase))
                                .AddIntent("mypartners", new Regex("partners", RegexOptions.IgnoreCase)));
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.EnableProactiveMessages = true;
                options.ConnectorClientRetryPolicy = new RetryPolicy(
                    new BotFrameworkHttpStatusCodeErrorDetectionStrategy(), 3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(1));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseBotFramework();
        }
    }
}
