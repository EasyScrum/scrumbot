using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Dialogs.Standup
{
    public class TicketStatusDialog : ComponentDialog
    {
        private List<TicketInfo> _tickets = null;

        public TicketStatusDialog()
            : base(nameof(TicketStatusDialog))
        {
            var slots = new List<SlotDetails>
            {
                new SlotDetails("doneStuff", "text", "What did you do?"),
                new SlotDetails("plans", "text", "What are you going to do?"),
            };

            var dialog = new WaterfallDialog("standup", new WaterfallStep[] {StartDialogAsync, ProcessResultsAsync });

            AddDialog(dialog);
            AddDialog(new SlotFillingDialog("workStuff", slots));
            AddDialog(new TextPrompt("text"));

            InitialDialogId = "standup";
        }

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as TicketStatusDialogOptions;
            if (options == null)
            {
                return await stepContext.EndDialogAsync();
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{options.User.FirstName}, please let us know your status for {options.Ticket.Name}."), cancellationToken);

            return await stepContext.BeginDialogAsync("workStuff", null, cancellationToken);
        }
        
        private async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as TicketStatusDialogOptions;
            TicketStatus ticketStatus = null;

            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {
                ticketStatus = new TicketStatus()
                {
                    TicketId = options.Ticket.Id,
                    DoneStuff = result["doneStuff"] as string,
                    FutureStuff = result["plans"] as string
                };

                var message = $"Post the following data to Jira for {options.Ticket.Name}: Done: {ticketStatus.DoneStuff}\nGoing to do: {ticketStatus.FutureStuff}";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
            }

            return await stepContext.EndDialogAsync(ticketStatus);
        }
    }
}
