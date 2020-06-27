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

            var dialog = new WaterfallDialog("standup", new WaterfallStep[] { StartDialogAsync, HandleUsers, FinishDialogAsync });

            //var users = await _issueTrackingIntegrationService.GetUsers();

            AddDialog(dialog);
            AddDialog(new StandupUsersProcessingDialog(userState, _issueTrackingIntegrationService));
            AddDialog(new TicketStatusDialog());
            AddDialog(new TicketsReviewDialog(_issueTrackingIntegrationService));
            //AddDialog(new TextPrompt("text"));

            InitialDialogId = "standup";
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Lets start the standup meeting."), cancellationToken);
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> HandleUsers(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var users = await _issueTrackingIntegrationService.GetUsers();
            // foreach (var user in users)
            // {
            //     await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Hi, {user.FirstName}!"), cancellationToken);
            // }

            return await stepContext.BeginDialogAsync(nameof(StandupUsersProcessingDialog), null, cancellationToken);
            //return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> FinishDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("The standup meeting completed."), cancellationToken);
            return await stepContext.EndDialogAsync();
        }
    }
}