// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EDMChatBot.Core;
using EDMChatBot.Core.AI;
using EDMChatBot.Core.Coverters;
using EDMChatBot.Core.Dialogs;
using EDMChatBot.Core.Dialogs.LanguageDialogs;
using EDMChatBot.Core.Dialogs.ViewSalesOrderDialogs;
using EDMChatBot.Core.Settings;
using EDMChatBot.Core.Storage;
using EDMChatBot.NAVClient;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Options;

namespace EDMChatBot
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddBot<ChatBot>(options =>
            {
                var botSettings = GetSettings<BotFileSettings>(Configuration);
                var botConfig = BotConfiguration.Load(botSettings.botFilePath , botSettings.botFileSecret);
                services.AddSingleton(botConfig);
                var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == "Azure Endpoint");
                var endpointService = service as EndpointService;
                options.CredentialProvider =
                    new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
                options.OnTurnError = async (context, exception) =>
                    {
                        await context.SendActivityAsync(exception.Message + "\n" + exception.StackTrace);
                    };
            });
            services.AddSingleton<ConversationState>(new ConversationState(new MemoryStorage()));
            services.AddSingleton<IOrderDialogSet, SalesOrderDialogSet>();
            services.AddSingleton<ILanguageDialogSet, LanguageDialogSet>();
            services.AddSingleton<IProductDialog, ProductDialog>();
            services.AddSingleton<IVoiceVerificationDialog, VoiceVerificationDialog>();
            services.AddSingleton<ICustomerIdDialog, CustomerIdDialogWithVoiceRecognition>();
            services.AddSingleton<ISpeechRecognizer>(sp =>
            {
                var settings = GetSettings<SpeechRecognizerSettings>(Configuration);
                return new SpeechHttpRecognizer(settings);
            });
            services.AddSingleton<IAudioConverter>(sp =>
            {
                var settings = GetSettings<CloudConverterSettings>(Configuration);
                return new CloudConverter(settings);
            });    
                
            services.AddSingleton<IViewOrdersDialogSet, ViewOrdersDialogSet>();
            services.AddSingleton<IViewOrdersDialog, ViewOrdersDialog>();
            services.AddSingleton<IAnotherProductDialog, AnotherProductDialog>();
            services.AddSingleton<ISalesOrderPostDialog, SalesOrderPostDialog>();
            services.AddSingleton<IChatter, ChatterWithTranslation>();
            services.AddSingleton<INaturalLanguageEngine>(SP =>
            {
                var settings = GetSettings<LuisSettings>(Configuration);
                return new LuisAIEngine(settings);
            });

            services.AddSingleton<IFaceClient>(sp =>
            {
                var settings = GetSettings<FaceApiSettings>(Configuration);
                var client = new FaceClient(new ApiKeyServiceClientCredentials(settings.SubscriptionKey));
                client.Endpoint = settings.EndpointUrl;
                return client;
            });
            services.AddSingleton<IFaceRecognizer>(sp =>
            {
                var settings = GetSettings<FaceApiSettings>(Configuration);
                return new FaceApiRecognizer(sp.GetRequiredService<IFaceClient>(), settings);
            });
            services.AddSingleton<SalesDialogBotAccessors>(sp =>
            {
                var conversationState = sp.GetRequiredService<ConversationState>();
               return new SalesDialogBotAccessors(conversationState)
                {
                    OrderDialogState = conversationState.CreateProperty<DialogState>("OrderDialogState"),
                    LanguageDialogState = conversationState.CreateProperty<DialogState>("LanguageDialogState"),
                    CustomerDataState = conversationState.CreateProperty<CustomerData>("CustomerData"),
                    ViewOrdersDialogState = conversationState.CreateProperty<DialogState>("ViewOrdersDialogState")
                };
            });
            services.AddSingleton<ITextTranslator>(sp =>
            {
                var settings = GetSettings<TranslatorApiSettings>(Configuration);
                return  new TextTranslator(settings);
            });
            services.AddSingleton<ILanguageDialog, LanguageDialog>();
            

            services.AddSingleton<INavClient>(provider =>
            {
                var settings = GetSettings<NavServerSettings>(Configuration);
                var creds = new CredentialCache();
                creds.Add(new Uri(settings.BaseUri), "NTLM",
                    new NetworkCredential(settings.Username, settings.Password));
                
                var client = new HttpClient(new HttpClientHandler
                {
                    Credentials = creds
                });
                client.BaseAddress = new Uri(settings.BaseUri);
                return new NavHttpClient(client,settings.CompanyId);
            });
            services.AddSingleton<ICloudTable>(sp =>
            {
                var settings = GetSettings<AzureTableSettings>(Configuration);
                return new AzureCloudTable(settings.ConnectionString, settings.TableName);
            });

            services.AddSingleton<IProfileStore, AzureTableProfileStore>();
            services.AddSingleton<IVoiceVerifier>(sp =>
            {
                var settings = GetSettings<VoiceVerifierSettings>(Configuration);
                return new VoiceHttpVerifier(settings, sp.GetRequiredService<IProfileStore>(),
                    sp.GetRequiredService<IAudioConverter>());
            });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseBotFramework();
        }
        
        public static T GetSettings<T>(IConfiguration configuration) where T : new()
        {
            var config = configuration.GetSection(typeof(T).Name);
            T settings = new T();
            config.Bind(settings);
            return settings;
        }
    }
}