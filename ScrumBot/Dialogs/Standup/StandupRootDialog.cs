using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Dialogs.Standup
{
    public class StandupRootDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<JObject> _userStateAccessor;
        private readonly IIssueTrackingIntegrationService _issueTrackingIntegrationService;

        public StandupRootDialog(UserState userState, IIssueTrackingIntegrationService issueTrackingIntegrationService)
            : base(nameof(StandupRootDialog))
        {
            _issueTrackingIntegrationService = issueTrackingIntegrationService;
            _userStateAccessor = userState.CreateProperty<JObject>("result");

            var dialog = new WaterfallDialog("standup", new WaterfallStep[]
            {
                StartDialogAsync, 
                FinishDialogAsync
            });

            //var users = await _issueTrackingIntegrationService.GetUsers();

            AddDialog(dialog);
            AddDialog(new TicketReviewDialog());
            AddDialog(new UserReviewDialog(_issueTrackingIntegrationService));
            AddDialog(new TeamReviewDialog(_issueTrackingIntegrationService));

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