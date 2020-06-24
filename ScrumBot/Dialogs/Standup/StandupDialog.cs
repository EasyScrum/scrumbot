using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;

namespace ScrumBot.Dialogs.Standup
{
    public class StandupDialog : ComponentDialog
    {
        private IStatePropertyAccessor<JObject> _userStateAccessor;

        public StandupDialog(UserState userState)
            : base(nameof(StandupDialog))
        {
            _userStateAccessor = userState.CreateProperty<JObject>("result");

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

        private async Task<DialogTurnResult> StartDialogAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Start the child dialog. This will just get the user's first and last name.
            return await stepContext.BeginDialogAsync("workStuff", null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessResultsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // To demonstrate that the slot dialog collected all the properties we will echo them back to the user.
            if (stepContext.Result is IDictionary<string, object> result && result.Count > 0)
            {
                // Now the waterfall is complete, save the data we have gathered into UserState.
                // This includes data returned by the adaptive dialog.
                var obj = await _userStateAccessor.GetAsync(stepContext.Context, () => new JObject());
                obj["data"] = new JObject
                {
                    { "workStuff",  $"Done: {result["doneStuff"]}\nGoing to do: {result["plans"]}" }
                };

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(obj["data"]["workStuff"].Value<string>()), cancellationToken);
            }

            // Remember to call EndAsync to indicate to the runtime that this is the end of our waterfall.
            return await stepContext.EndDialogAsync();
        }
    }
}
