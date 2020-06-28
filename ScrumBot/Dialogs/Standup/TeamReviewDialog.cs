﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Dialogs.Standup
{
    public class TeamReviewDialog : ComponentDialog
    {
        private const string ReportedUsersKey = "value-usersReported";
        private const string UserInfosKey = "value-userInfos";

        private readonly IIssueTrackingIntegrationService _issueTrackingIntegrationService;

        public TeamReviewDialog(IIssueTrackingIntegrationService issueTrackingIntegrationService)
            : base(nameof(TeamReviewDialog))
        {
            _issueTrackingIntegrationService = issueTrackingIntegrationService;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                Init,
                SelectionStep,
                LoopStep,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private UsersReviewDialogOptions GetOptions(WaterfallStepContext stepContext)
        {
            return stepContext.Options as UsersReviewDialogOptions ?? new UsersReviewDialogOptions();
        }

        private async Task<DialogTurnResult> Init(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            var users = options.Users ?? (await _issueTrackingIntegrationService.GetUsers())?.ToList();
            stepContext.Values[UserInfosKey] = users;

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> SelectionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            var reportedUsers = options.ReportedUsers ?? new List<string>();
            stepContext.Values[ReportedUsersKey] = reportedUsers;

            var users = stepContext.Values[UserInfosKey] as List<UserInfo> ?? new List<UserInfo>();
            var user = users.FirstOrDefault(x => !reportedUsers.Contains(x.Id));
            if (user != null)
            {
                reportedUsers.Add(user.Id);

                var ticketsReviewDialogOptions = new TicketsReviewDialogOptions() { User = user };
                return await stepContext.BeginDialogAsync(nameof(UserReviewDialog), ticketsReviewDialogOptions, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> LoopStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reportedUsers = stepContext.Values[ReportedUsersKey] as List<string> ?? new List<string>();
            var users = stepContext.Values[UserInfosKey] as List<UserInfo> ?? new List<UserInfo>();
            var options = new UsersReviewDialogOptions
            {
                Users = users,
                ReportedUsers = reportedUsers
            };
            return await stepContext.ReplaceDialogAsync(nameof(TeamReviewDialog), options, cancellationToken);
        }
    }
}