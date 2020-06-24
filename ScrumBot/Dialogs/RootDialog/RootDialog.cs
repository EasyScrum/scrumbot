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
using ScrumBot.Dialogs.Standup;
using ScrumBot.Utils;

namespace ScrumBot.Dialogs.RootDialog
{
    public class RootDialog : ComponentDialog
    {
        private static IConfiguration _configuration;
        private static UserState _userState;

        public RootDialog(IConfiguration configuration, UserState userState)
            : base(nameof(RootDialog))
        {
            _configuration = configuration;
            _userState = userState;

            string[] paths = { ".", "Dialogs", "RootDialog", "RootDialog.lg" };
            string fullPath = Path.Combine(paths);

            // Create instance of adaptive dialog. 
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(Templates.ParseFile(fullPath)),
                Recognizer = RegexRecognizerHelper.CreateTestRecognizer(),
                Triggers = new List<OnCondition>()
                {
                    // Add a rule to welcome user
                    new OnConversationUpdateActivity()
                    {
                        Actions = WelcomeUserSteps()
                    },
                    new OnIntent("Greeting")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${HelpRootDialog()}")
                        }
                    },
                    new OnIntent("Standup")
                    {
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog(nameof(StandupDialog))
                            //new SendActivity("${NotAvailableFunctionalityMessage()}")
                        }
                    },
                    new OnIntent("Planning")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${NotAvailableFunctionalityMessage()}")
                        }
                    },
                    new OnIntent("Grooming")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${NotAvailableFunctionalityMessage()}")
                        }
                    },
                    new OnIntent("Setup")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${NotAvailableFunctionalityMessage()}")
                        }
                    },
                    new OnIntent("Help")
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("${HelpRootDialog()}")
                        }
                    },
                    new OnIntent("Cancel")
                    {
                        Actions = new List<Dialog>()
                        {
                            // Ask user for confirmation.
                            // This input will still use the recognizer and specifically the confirm list entity extraction.
                            new ConfirmInput()
                            {
                                Prompt = new ActivityTemplate("${Cancel.prompt()}"),
                                Property = "turn.confirm",
                                Value = "=@confirmation",
                                // Allow user to intrrupt this only if we did not get a value for confirmation.
                                AllowInterruptions = "!@confirmation"
                            },
                            new IfCondition()
                            {
                                Condition = "turn.confirm == true",
                                Actions = new List<Dialog>()
                                {
                                    // This is the global cancel in case a child dialog did not explicit handle cancel.
                                    new SendActivity("Cancelling all dialogs.."),
                                    // SendActivity supports full language generation resolution.
                                    // See here to learn more about language generation
                                    // https://aka.ms/language-generation
                                    new SendActivity("${WelcomeActions()}"),
                                    new CancelAllDialogs(),
                                },
                                ElseActions = new List<Dialog>()
                                {
                                    new SendActivity("${CancelCancelled()}"),
                                    new SendActivity("${WelcomeActions()}")
                                }
                            }

                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            AddDialog(new StandupDialog(_userState));
            // AddDialog(new StandupUserDialog());
            // AddDialog(new StandupMeetingDialog());

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static List<Dialog> WelcomeUserSteps()
        {
            return new List<Dialog>()
            {
                new SetProperties()
                {
                    Assignments = new List<PropertyAssignment>()
                    {
                        new PropertyAssignment()
                        {
                            Property = "user.profile.name",
                            Value = "=coalesce(dialog.userName, @userName, @personName)"
                        }
                    }
                },

                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition()
                        {
                            Condition = "$foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("${IntroMessage()}")
                            }
                        }
                    }
                }
            };
        }
    }
}
