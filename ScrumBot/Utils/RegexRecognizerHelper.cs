using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace ScrumBot.Utils
{
    public static class RegexRecognizerHelper
    {
        public static Recognizer CreateTestRecognizer()
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