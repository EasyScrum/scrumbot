using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Dialogs.Standup
{
    public class StandupUsersProcessingDialog : Dialog
    {
        // Custom dialogs might define their own custom state.
        // Similarly to the Waterfall dialog we will have a set of values in the ConversationState. However, rather than persisting
        // an index we will persist the last property we prompted for. This way when we resume this code following a prompt we will
        // have remembered what property we were filling.
        private const string UserName = "username";
        private const string PersistedValues = "uservalues";

        private readonly UserState _userState;
        private readonly IIssueTrackingIntegrationService _issueTrackingIntegrationService;
        private List<UserInfo> _users;

        public StandupUsersProcessingDialog(UserState userState,
            IIssueTrackingIntegrationService issueTrackingIntegrationService)
            : base(nameof(StandupUsersProcessingDialog))
        {
            _userState = userState;
            _issueTrackingIntegrationService = issueTrackingIntegrationService;
        }

        /// <summary>
        /// Begin is called to start a new slot dialog.
        /// </summary>
        /// <param name="dialogContext">A handle on the runtime.</param>
        /// <param name="options">This isn't used in this implementation but required for the contract. Potentially it could be used to pass in existing state - already filled slots for example.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DialogTurnResult indicating the state of this dialog to the caller.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            _users ??= (await _issueTrackingIntegrationService.GetUsers())?.ToList();

            // Run prompt
            return await RunPromptAsync(dialogContext, cancellationToken);
        }

        /// <summary>
        /// Continue is called to run an existing dialog. It will return the state of the current dialog. If there is no dialog it will return Empty.
        /// </summary>
        /// <param name="dialogContext">A handle on the runtime.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DialogTurnResult indicating the state of this dialog to the caller.</returns>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            // Don't do anything for non-message activities.
            if (dialogContext.Context.Activity.Type != ActivityTypes.Message)
            {
                return EndOfTurn;
            }

            // Run next step with the message text as the result.
            return await RunPromptAsync(dialogContext, cancellationToken);
        }

        /// <summary>
        /// Resume is called when a child dialog completes and we need to carry on processing in this class.
        /// </summary>
        /// <param name="dialogContext">A handle on the runtime.</param>
        /// <param name="reason">The reason we have control back in this dialog.</param>
        /// <param name="result">The result from the child dialog. For example this is the value from a prompt.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DialogTurnResult indicating the state of this dialog to the caller.</returns>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dialogContext, DialogReason reason, object result, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            // Update the state with the result from the child prompt.
            var userName = (string)dialogContext.ActiveDialog.State[UserName];
            var values = GetPersistedValues(dialogContext.ActiveDialog);
            values[userName] = result;


            // Run prompt.
            return await RunPromptAsync(dialogContext, cancellationToken);
        }

        /// <summary>
        /// Helper function to deal with the persisted values we collect in this dialog.
        /// </summary>
        /// <param name="dialogInstance">A handle on the runtime instance associated with this dialog, the State is a property.</param>
        /// <returns>A dictionary representing the current state or a new dictionary if we have none.</returns>
        private static IDictionary<string, object> GetPersistedValues(DialogInstance dialogInstance)
        {
            object obj;
            if (!dialogInstance.State.TryGetValue(PersistedValues, out obj))
            {
                obj = new Dictionary<string, object>();
                dialogInstance.State.Add(PersistedValues, obj);
            }

            return (IDictionary<string, object>)obj;
        }

        /// <summary>
        /// This helper function contains the core logic of this dialog. The main idea is to compare the state we have gathered with the
        /// list of slots we have been asked to fill. When we find an empty slot we execute the corresponding prompt.
        /// </summary>
        /// <param name="dialogContext">A handle on the runtime.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DialogTurnResult indicating the state of this dialog to the caller.</returns>
        private async Task<DialogTurnResult> RunPromptAsync(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            var state = GetPersistedValues(dialogContext.ActiveDialog);

            // Run through the list of slots until we find one that hasn't been filled yet.
            var unfilledUser = _users.FirstOrDefault((item) => !state.ContainsKey(item.Id));

            // If we have an unfilled slot we will try to fill it
            if (unfilledUser != null)
            {
                dialogContext.ActiveDialog.State[UserName] = unfilledUser.Id;

                var ticketsReviewDialogOptions = new TicketsReviewDialogOptions() {User = unfilledUser};
                return await dialogContext.BeginDialogAsync(nameof(TicketsReviewDialog), ticketsReviewDialogOptions, cancellationToken);
            }
            else
            {
                // No more slots to fill so end the dialog.
                return await dialogContext.EndDialogAsync(state);
            }
        }
    }
}
