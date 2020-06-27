﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Dialogs.Standup
{
    public class TicketsReviewDialog : ComponentDialog
    {
        private const string DoneOption = "done";
        private const string ReportedTicketsKey = "value-ticketsReported";
        private const string UserKey = "value-user";
        private const string TicketInfosKey = "value-ticketInfos";

        private readonly IIssueTrackingIntegrationService _issueTrackingIntegrationService;

        public TicketsReviewDialog(IIssueTrackingIntegrationService issueTrackingIntegrationService)
            : base(nameof(TicketsReviewDialog))
        {
            _issueTrackingIntegrationService = issueTrackingIntegrationService;
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                Init,
                SelectionStep,
                ProcessTicket,
                LoopStep,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private TicketsReviewDialogOptions GetOptions(WaterfallStepContext stepContext)
        {
            return stepContext.Options as TicketsReviewDialogOptions ?? new TicketsReviewDialogOptions();
        }

        private async Task<DialogTurnResult> Init(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            if (options?.User == null)
            {
                return await stepContext.EndDialogAsync();
            }

            stepContext.Values[UserKey] = options.User;

            var tickets = options.Tickets ?? (await _issueTrackingIntegrationService.GetUserTickets(options.User.Id))?.ToList();
            stepContext.Values[TicketInfosKey] = tickets;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"[start new thread here] Hi, {options.User.FirstName}, please let us know your status for today."), cancellationToken);

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SelectionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            var reportedTickets = options.ReportedTickets ?? new List<string>();
            stepContext.Values[ReportedTicketsKey] = reportedTickets;

            var tickets = stepContext.Values[TicketInfosKey] as List<TicketInfo> ?? new List<TicketInfo>();
            var choiceOptions = tickets.Where(x => !reportedTickets.Contains(x.Id)).Select(GetTicketOption).ToList();
            choiceOptions.Add(DoneOption);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text($"Please choose a ticket to report, or `{DoneOption}` to finish."),
                RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                Choices = ChoiceFactory.ToChoices(choiceOptions),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessTicket(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reportedTickets = stepContext.Values[ReportedTicketsKey] as List<string> ?? new List<string>();
            var choice = (FoundChoice)stepContext.Result;
            var done = choice.Value == DoneOption;
            var tickets = stepContext.Values[TicketInfosKey] as List<TicketInfo> ?? new List<TicketInfo>();

            if (!done)
            {
                var ticket = tickets.FirstOrDefault(x => GetTicketOption(x) == choice.Value);
                if (ticket != null)
                {
                    reportedTickets.Add(ticket.Id);

                    var ticketStatusDialogOptions = new TicketStatusDialogOptions()
                    {
                        User = GetOptions(stepContext).User,
                        Ticket = ticket
                    };
                    return await stepContext.BeginDialogAsync(nameof(TicketStatusDialog), ticketStatusDialogOptions, cancellationToken);
                }
                else
                {
                    return await RepeatDialog(stepContext, cancellationToken, tickets, reportedTickets);
                }
            }

            return await stepContext.EndDialogAsync(reportedTickets, cancellationToken);
        }

        private async Task<DialogTurnResult> LoopStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reportedTickets = stepContext.Values[ReportedTicketsKey] as List<string> ?? new List<string>();
            var ticketStatus = stepContext.Result as TicketStatus;
            var tickets = stepContext.Values[TicketInfosKey] as List<TicketInfo> ?? new List<TicketInfo>();

            if (ticketStatus != null)
            {
                await _issueTrackingIntegrationService.SubmitComment(ticketStatus.TicketId,
                    $"Done: {ticketStatus.DoneStuff}. \nPlans: {ticketStatus.FutureStuff}");
            }

            return await RepeatDialog(stepContext, cancellationToken, tickets, reportedTickets);
        }

        private async Task<DialogTurnResult> RepeatDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken,
            List<TicketInfo> tickets, List<string> reportedTickets)
        {
            var options = new TicketsReviewDialogOptions
            {
                User = stepContext.Values[UserKey] as UserInfo,
                ReportedTickets = reportedTickets,
                Tickets = tickets
            };
            return await stepContext.ReplaceDialogAsync(nameof(TicketsReviewDialog), options, cancellationToken);
        }
        
        private string GetTicketOption(TicketInfo ticket)
        {
            return $"{ticket.Name} - {ticket.Title}";
        }
    }
}