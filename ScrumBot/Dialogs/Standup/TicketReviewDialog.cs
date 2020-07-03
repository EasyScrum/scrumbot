using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using ScrumBot.Models;
using ScrumBot.Services;
using ScrumBot.Utils;

namespace ScrumBot.Dialogs.Standup
{
    public class TicketReviewDialog : ComponentDialog
    {
        private const string DoneStuffKey = "value-donestuff";
        private const string FutureStuffKey = "value-futuretuff";

        private readonly SettingProvider _settingProvider;

        public TicketReviewDialog(SettingProvider settingProvider)
            : base(nameof(TicketReviewDialog))
        {
            _settingProvider = settingProvider;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskDoneStep,
                AskPlanStep,
                ProcessResultStep,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private TicketStatusDialogOptions GetOptions(WaterfallStepContext stepContext)
        {
            return stepContext.Options as TicketStatusDialogOptions ?? new TicketStatusDialogOptions();
        }

        private async Task<DialogTurnResult> AskDoneStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);
            var prompt = DialogHelper.GetMessageActivityWithMention(options.User, $"{{0}}, please let us know your status for {options.Ticket.Name}.\n\nWhat did you do?", _settingProvider.UseTeams);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = prompt }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskPlanStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            if (!DialogHelper.IsExpectedUser(options.User, stepContext.Context.Activity.From, _settingProvider.UseTeams))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Waiting response from {DialogHelper.GetUserFullName(options.User)}"), cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(TicketReviewDialog), options, cancellationToken);
            }

            HandleResult(stepContext, DoneStuffKey);

            var prompt = DialogHelper.GetMessageActivityWithMention(options.User, $"{{0}}, what are you going to do for {options.Ticket.Name}?", _settingProvider.UseTeams);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = prompt }, cancellationToken);
        }

        private void HandleResult(WaterfallStepContext stepContext, string storeValueKey)
        {
            var result = stepContext.Result as string;
            if (result != null)
            {
                var mentions = stepContext.Context.Activity.GetMentions();
                if (mentions != null && mentions.Length > 0)
                {
                    result = result.Replace(mentions[0].Text, "").Trim();
                }

                stepContext.Values[storeValueKey] = result;
            }
        }

        private async Task<DialogTurnResult> ProcessResultStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            if (!DialogHelper.IsExpectedUser(options.User, stepContext.Context.Activity.From, _settingProvider.UseTeams))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Waiting response from {DialogHelper.GetUserFullName(options.User)}"), cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(TicketReviewDialog), options, cancellationToken);
            }

            HandleResult(stepContext, FutureStuffKey);

            var ticketStatus = new TicketStatus()
            {
                TicketId = options.Ticket.Id,
                DoneStuff = stepContext.Values[DoneStuffKey] as string,
                FutureStuff = stepContext.Values[FutureStuffKey] as string
            };

            var message = $"The following update will be posted data to Jira for {options.Ticket.Name}:\n\n- {ticketStatus.DoneStuff}\n\n- {ticketStatus.FutureStuff}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);

            return await stepContext.EndDialogAsync(ticketStatus);
        }
    }
}
