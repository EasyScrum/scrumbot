using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.AI.Luis;

namespace ScrumBot.Utils
{
    public static class RecognizerFactory
    {
        public static Recognizer CreateRecognizer(IConfiguration configuration)
        {
            //return CreateLuisRecognizer(configuration);
            return CreateRegexRecognizer();
        }

        public static Recognizer CreateLuisRecognizer(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["LuisAppId"]) || string.IsNullOrEmpty(configuration["LuisAPIKey"]) || string.IsNullOrEmpty(configuration["LuisAPIHostName"]))
            {
                throw new Exception("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.");
            }

            return new LuisAdaptiveRecognizer()
            {
                ApplicationId = configuration["LuisAppId"],
                EndpointKey = configuration["LuisAPIKey"],
                Endpoint = configuration["LuisAPIHostName"]
            };
        }

        public static Recognizer CreateRegexRecognizer()
        {
            return new RegexRecognizer()
            {
                Intents = new List<IntentPattern>()
                {
                    new IntentPattern()
                    {
                        Intent = "ReportTicket",
                        Pattern = "(?i)\\w{3}-\\d{1,6}\\s*"
                    },
                    new IntentPattern()
                    {
                        Intent = "Standup",
                        Pattern = "(?i)standup|stand-up|daily meeting"
                    },
                    new IntentPattern()
                    {
                        Intent = "Help",
                        Pattern = "(?i)help"
                    },
                    new IntentPattern()
                    {
                        Intent = "Cancel",
                        Pattern = "(?i)cancel|never mind"
                    },
                    new IntentPattern()
                    {
                        Intent = "Setup",
                        Pattern = "(?i)settings|setup|set up|initialize|configure"
                    },
                    new IntentPattern()
                    {
                        Intent = "Planning",
                        Pattern = "(?i)planning"
                    },
                    new IntentPattern()
                    {
                        Intent = "Grooming",
                        Pattern = "(?i)grooming|refinement"
                    },
                    new IntentPattern()
                    {
                        Intent = "MainMenu",
                        Pattern = "(?i)menu|main menu|root"
                    },
                    new IntentPattern()
                    {
                        Intent = "Root",
                        Pattern = "(?i)hello|start"
                    },
                },
                Entities = new List<EntityRecognizer>()
                {
                    new ConfirmationEntityRecognizer(),
                    new DateTimeEntityRecognizer(),
                    new NumberEntityRecognizer()
                }
            };
        }
    }
}