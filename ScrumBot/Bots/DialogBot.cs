using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Extensions.Logging;

namespace ScrumBot.Bots
{
    public class DialogBot<T> : TeamsActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        protected readonly DialogManager DialogManager;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
            DialogManager = new DialogManager(Dialog);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.LogInformation("Running dialog with Activity.");
            await DialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);

            // await base.OnTurnAsync(turnContext, cancellationToken);
            //
            // // Save any state changes that might have occurred during the turn.
            // await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            // await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        // protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        // {
        //     Logger.LogInformation("Running dialog with Message Activity.");
        //
        //     // Run the Dialog with the new message Activity.
        //     await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        // }
    }
}
