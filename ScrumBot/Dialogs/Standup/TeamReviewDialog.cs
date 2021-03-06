﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using ScrumBot.Contracts;
using ScrumBot.Models;
using ScrumBot.Services;

namespace ScrumBot.Dialogs.Standup
{
    public class TeamReviewDialog : ComponentDialog
    {
        private const string ReportedUsersKey = "value-usersReported";
        private const string UserInfosKey = "value-userInfos";

        private readonly IIssueTrackingIntegrationService _issueTrackingIntegrationService;
        private readonly SettingProvider _settingProvider;

        public TeamReviewDialog(IIssueTrackingIntegrationService issueTrackingIntegrationService, SettingProvider settingProvider)
            : base(nameof(TeamReviewDialog))
        {
            _issueTrackingIntegrationService = issueTrackingIntegrationService;
            _settingProvider = settingProvider;

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

            var users = options.Users ?? await InitUserList(stepContext, cancellationToken);
            stepContext.Values[UserInfosKey] = users;

            return await stepContext.NextAsync();
        }

        private async Task<List<UserDetails>> InitUserList(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (_settingProvider.UseTeams)
            {
                var members = await TeamsInfo.GetMembersAsync(stepContext.Context, cancellationToken);

                return members.Select(x => new UserDetails()
                {
                    Id = x.Id,
                    FirstName = x.GivenName,
                    Lastname = x.Surname,
                    Email = x.Email,
                    TeamsUserInfo = x
                }).ToList();
            }
            else
            {
                var users = await _issueTrackingIntegrationService.GetUsers();
                return users.ToList();
            }
        }

        private async Task<DialogTurnResult> SelectionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = GetOptions(stepContext);

            var reportedUsers = options.ReportedUsers ?? new List<string>();
            stepContext.Values[ReportedUsersKey] = reportedUsers;

            var users = stepContext.Values[UserInfosKey] as List<UserDetails> ?? new List<UserDetails>();
            var user = users.FirstOrDefault(x => !reportedUsers.Contains(x.Email));
            if (user != null)
            {
                reportedUsers.Add(user.Email);

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
            var users = stepContext.Values[UserInfosKey] as List<UserDetails> ?? new List<UserDetails>();
            var options = new UsersReviewDialogOptions
            {
                Users = users,
                ReportedUsers = reportedUsers
            };
            return await stepContext.ReplaceDialogAsync(nameof(TeamReviewDialog), options, cancellationToken);
        }
    }
}