using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Services
{
    public class JiraService : IJiraService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public JiraService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> TryAddCommentAsync(string issueId, UserComment comment)
        {
            var client = httpClientFactory.CreateClient(Startup.JiraClientName);
            var usersData = await client.PostAsync(
                new Uri($"/rest/api/3/issue/{issueId}/comment", UriKind.Relative),
                new StringContent(JsonConvert.SerializeObject(comment), Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            return usersData.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<UserInfo>> GetUsersAsync(string projectId)
        {
            var client = httpClientFactory.CreateClient(Startup.JiraClientName);
            var usersData = await client.GetAsync(
                new Uri($"/rest/api/3/user/assignable/multiProjectSearch?projectKeys={projectId}", UriKind.Relative))
                .ConfigureAwait(false);
            IEnumerable<UserInfo> result = new List<UserInfo>();

            if (usersData.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<IEnumerable<UserInfo>>(await usersData.Content.ReadAsStringAsync());
            }

            return result;
        }

        public async Task<UserIssues> GetIssuesAsync(string assigneeId, string status)
        {
            var client = httpClientFactory.CreateClient(Startup.JiraClientName);
            var usersData = await client.GetAsync(
                new Uri($"/rest/api/3/search?jql=assignee={assigneeId} and status in (\"{status}\")", UriKind.Relative))
                .ConfigureAwait(false);
            UserIssues result = new UserIssues();

            if (usersData.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<UserIssues>(await usersData.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return result;
        }
    }
}