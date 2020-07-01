using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScrumBot.Bots;
using ScrumBot.Contracts;
using ScrumBot.Dialogs.RootDialog;
using ScrumBot.Services;

namespace ScrumBot
{
    public class Startup
    {
        public const string JiraClientName = "jira";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            ComponentRegistration.Add(new DialogsComponentRegistration());

            ComponentRegistration.Add(new AdaptiveComponentRegistration());

            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());

            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<IStorage, MemoryStorage>();

            services.AddSingleton<UserState>();

            services.AddSingleton<ConversationState>();
            
            services.AddSingleton<RootDialog>();

            services.AddSingleton<IIssueTrackingIntegrationService, JiraIntegrationService>();
            
            services.AddTransient<IBot, DialogBot<RootDialog>>();

            AddJiraService(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

        private void AddJiraService(IServiceCollection services)
        {
            services.AddHttpClient(JiraClientName, c =>
            {
                c.BaseAddress = new Uri("https://easyscrum.atlassian.net");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "a3J1dG9sZXZpY2gtbUB5YW5kZXgucnU6blRmT1Zya0EyOXNFSkNxeVRpZEdFNzJE");
            });
            services.AddSingleton<IJiraService, JiraService>();
        }
    }
}
