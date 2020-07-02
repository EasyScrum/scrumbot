using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Services
{
    public class JiraIntegrationService : IIssueTrackingIntegrationService
    {
        private readonly IJiraService jiraService;

        public JiraIntegrationService(IJiraService jiraService)
        {
            this.jiraService = jiraService;
        }

        public async Task<IEnumerable<UserDetails>> GetUsers()
        {
            var users = await this.jiraService.GetUsersAsync("EAS").ConfigureAwait(false);

            return users.Select(user => new UserDetails()
            {
                Email = user.EmailAddress,
                FirstName = user.DisplayName,
                Lastname = user.DisplayName,
                Id = user.AccountId
            })
                .ToList();
        }

        public async Task<IEnumerable<TicketInfo>> GetUserActiveTickets(string userId)
        {
            var issues = await this.jiraService.GetIssuesAsync(userId, "In Progress").ConfigureAwait(false);

            return issues.Issues.Select(issue => new Models.TicketInfo()
            {
                AssigneeId = userId,
                Id = issue.Key,
                Name = issue.Key,
                Title = issue.Fields.Summary
            })
                .ToList();
        }

        public async Task<IEnumerable<TicketInfo>> GetTicketsByUserEmail(string userEmail)
        {
            var users = await GetUsers();
            var user = users.FirstOrDefault(x => string.Equals(x.Email, userEmail, StringComparison.InvariantCultureIgnoreCase));

            if (user == null) return new List<TicketInfo>();

            return await GetUserActiveTickets(user.Id);
        }

        public async Task<bool> SubmitComment(string ticketId, string comment)
        {
            var userComment = new UserComment
            {
                Body = new Body()
                {
                    Version = 1,
                    Type = "doc",
                    Content = new List<Content>()
                    {
                        new Content()
                        {
                            ContentItems = new List<ContentItem>()
                                {
                                    new ContentItem()
                                    {
                                        Text = comment, Type = "text"
                                    }
                                },
                            Type = "paragraph"
                        }
                    }
                }
            };

            return await this.jiraService.TryAddCommentAsync(ticketId, userComment).ConfigureAwait(false);
        }
    }
}
