using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using ScrumBot.Contracts;

namespace ScrumBot.Dialogs.Standup
{
    public class StandupRootDialog : ComponentDialog
    {
        private readonly IIssueTrackingIntegrationService _issueTrackingIntegrationService;

        public StandupRootDialog(IIssueTrackingIntegrationService issueTrackingIntegrationService)
            : base(nameof(StandupRootDialog))
        {
            _issueTrackingIntegrationService = issueTrackingIntegrationService;

            var dialog = new WaterfallDialog("standup", new WaterfallStep[]
            {
                StartDialogAsync, 
                FinishDialogAsync
            });

            AddDialog(dialog);
            AddDialog(new TicketReviewDialog());
            AddDialog(new UserReviewDialog(_issueTrackingIntegrationService));
            AddDialog(new TeamReviewDialog(_issueTrackingIntegrationService));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = "standup";
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Lets start the standup meeting."), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(TeamReviewDialog), null, cancellationToken);
        }
        
        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("The standup meeting completed."), cancellationToken);
            return await stepContext.EndDialogAsync();
        }
    }
}