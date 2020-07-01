using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using ScrumBot.Contracts;
using ScrumBot.Dialogs.Standup;
using ScrumBot.Utils;

namespace ScrumBot.Dialogs.RootDialog
{
    public class RootDialog : ComponentDialog
    {
        public RootDialog(IConfiguration configuration, IIssueTrackingIntegrationService issueTrackingIntegrationService)
            : base(nameof(RootDialog))
        {
            string[] paths = { ".", "Dialogs", "RootDialog", "RootDialog.lg" };
            string fullPath = Path.Combine(paths);

            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                Recognizer = RecognizerFactory.CreateRecognizer(configuration),
                Triggers = new List<OnCondition>()
                {
                    new OnConversationUpdateActivity()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${WelcomeCard()}")
                        }
                    },
                    new OnIntent("Root")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${WelcomeCard()}")
                        }
                    },
                    new OnIntent("MainMenu")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${MainMenuCard()}")
                        }
                    },
                    new OnIntent("Standup")
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(nameof(StandupRootDialog)),
                            new SendActivity("${MainMenuCard()}")
                        }
                    },
                    new OnIntent("Planning")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${NotAvailableFunctionalityMessage()}"),
                            new SendActivity("${MainMenuCard()}")
                        }
                    },
                    new OnIntent("Grooming")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${NotAvailableFunctionalityMessage()}"),
                            new SendActivity("${MainMenuCard()}")
                        }
                    },
                    new OnIntent("Setup")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${NotAvailableFunctionalityMessage()}"),
                            new SendActivity("${MainMenuCard()}")
                        }
                    },
                    new OnIntent("Help")
                    {
                        Condition = "#Help.Score >= 0.8",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${HelpCard()}")
                        }
                    },
                    new OnIntent("Cancel")
                    {
                        Condition = "#Cancel.Score >= 0.8",
                        Actions = new List<Dialog>()
                        {
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("${Cancel.prompt()}"),
                                Property = "turn.confirm",
                                Value = "=@confirmation",
                                AllowInterruptions = "!@confirmation"
                            },
                            new IfCondition()
                            {
                                Condition = "turn.confirm == true",
                                Actions = new List<Dialog>()
                                {
                                    new SendActivity("Cancelling all dialogs.."),
                                    new SendActivity("${MainMenuCard()}"),
                                    new CancelAllDialogs(),
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${CancelCancelled()}"),
                                    new SendActivity("${MainMenuCard()}")
                                }
                            }

                        }
                    }
                }
            };

            AddDialog(rootDialog);
            AddDialog(new StandupRootDialog( issueTrackingIntegrationService));

            InitialDialogId = nameof(AdaptiveDialog);
        }
    }
}
