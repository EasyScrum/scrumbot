using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ScrumBot.vs
{
    public interface IJiraService
    {
        Task<IEnumerable<UserInfo>> GetUsersAsync(string projectId);
        Task<UserIssues> GetIssuesAsync(string assigneeId);
        Task<bool> TryAddCommentAsync(string issueId, UserComment comment);
    }

    public class JiraService : IJiraService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public JiraService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> TryAddCommentAsync(string issueId, UserComment comment)
        {
            var client = httpClientFactory.CreateClient(StartupHelper.ClientName);
            var usersData = await client.PostAsync(
                new Uri($"/rest/api/3/issue/{issueId}/comment", UriKind.Relative),
                new StringContent(JsonConvert.SerializeObject(comment), Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            return usersData.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<UserInfo>> GetUsersAsync(string projectId)
        {
            var client = httpClientFactory.CreateClient(StartupHelper.ClientName);
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

        public async Task<UserIssues> GetIssuesAsync(string assigneeId)
        {
            var client = httpClientFactory.CreateClient(StartupHelper.ClientName);
            var usersData = await client.GetAsync(
                new Uri($"/rest/api/3/search?jql=assignee={assigneeId}", UriKind.Relative))
                .ConfigureAwait(false);
            UserIssues result = new UserIssues();

            if (usersData.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<UserIssues>(await usersData.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return result;
        }
    }

    [DataContract]
    public class Issuetype
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "subtask")]
        public bool Subtask { get; set; }

        [DataMember(Name = "avatarId")]
        public int AvatarId { get; set; }
    }


    [DataContract]
    public class Project
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "projectTypeKey")]
        public string ProjectTypeKey { get; set; }

        [DataMember(Name = "simplified")]
        public bool Simplified { get; set; }

    }

    [DataContract]
    public class Watches
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "watchCount")]
        public int WatchCount { get; set; }

        [DataMember(Name = "isWatching")]
        public bool IsWatching { get; set; }
    }

    [DataContract]
    public class Priority
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }
    }

    [DataContract]
    public class NonEditableReason
    {

        [DataMember(Name = "reason")]
        public string Reason { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }

    [DataContract]
    public class Customfield10018
    {

        [DataMember(Name = "hasEpicLinkFieldDependency")]
        public bool HasEpicLinkFieldDependency { get; set; }

        [DataMember(Name = "showField")]
        public bool ShowField { get; set; }

        [DataMember(Name = "nonEditableReason")]
        public NonEditableReason NonEditableReason { get; set; }
    }

    [DataContract]
    public class Assignee
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "accountId")]
        public string AccountId { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "timeZone")]
        public string TimeZone { get; set; }

        [DataMember(Name = "accountType")]
        public string AccountType { get; set; }
    }

    [DataContract]
    public class StatusCategory
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "colorName")]
        public string ColorName { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class Status
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "statusCategory")]
        public StatusCategory StatusCategory { get; set; }
    }

    [DataContract]
    public class Creator
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "accountId")]
        public string AccountId { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "timeZone")]
        public string TimeZone { get; set; }

        [DataMember(Name = "accountType")]
        public string AccountType { get; set; }
    }

    [DataContract]
    public class Reporter
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "accountId")]
        public string AccountId { get; set; }

        [DataMember(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "timeZone")]
        public string TimeZone { get; set; }

        [DataMember(Name = "accountType")]
        public string AccountType { get; set; }
    }

    [DataContract]
    public class AggregateProgress
    {

        [DataMember(Name = "progress")]
        public int Progress { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }
    }

    [DataContract]
    public class ProgressItem
    {

        [DataMember(Name = "progress")]
        public int Progress { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }
    }

    [DataContract]
    public class VotesItem
    {

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "votes")]
        public int Votes { get; set; }

        [DataMember(Name = "hasVoted")]
        public bool HasVoted { get; set; }
    }

    [DataContract]
    public class Fields
    {

        [DataMember(Name = "statuscategorychangedate")]
        public DateTime Statuscategorychangedate { get; set; }

        [DataMember(Name = "issuetype")]
        public Issuetype Issuetype { get; set; }

        [DataMember(Name = "timespent")]
        public object Timespent { get; set; }

        [DataMember(Name = "project")]
        public Project Project { get; set; }

        [DataMember(Name = "fixVersions")]
        public IList<object> FixVersions { get; set; }

        [DataMember(Name = "aggregatetimespent")]
        public object Aggregatetimespent { get; set; }

        [DataMember(Name = "resolution")]
        public object Resolution { get; set; }

        [DataMember(Name = "resolutiondate")]
        public object Resolutiondate { get; set; }

        [DataMember(Name = "workratio")]
        public int Workratio { get; set; }

        [DataMember(Name = "watches")]
        public Watches Watches { get; set; }

        [DataMember(Name = "lastViewed")]
        public DateTime LastViewed { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "customfield_10020")]
        public IList<string> Customfield10020 { get; set; }

        [DataMember(Name = "customfield_10021")]
        public object Customfield10021 { get; set; }

        [DataMember(Name = "customfield_10022")]
        public object Customfield10022 { get; set; }

        [DataMember(Name = "customfield_10023")]
        public object Customfield10023 { get; set; }

        [DataMember(Name = "priority")]
        public Priority Priority { get; set; }

        [DataMember(Name = "customfield_10024")]
        public object Customfield10024 { get; set; }

        [DataMember(Name = "customfield_10025")]
        public object Customfield10025 { get; set; }

        [DataMember(Name = "customfield_10026")]
        public object Customfield10026 { get; set; }

        [DataMember(Name = "labels")]
        public IList<object> Labels { get; set; }

        [DataMember(Name = "customfield_10016")]
        public object Customfield10016 { get; set; }

        [DataMember(Name = "customfield_10017")]
        public object Customfield10017 { get; set; }

        [DataMember(Name = "customfield_10018")]
        public Customfield10018 Customfield10018 { get; set; }

        [DataMember(Name = "customfield_10019")]
        public string Customfield10019 { get; set; }

        [DataMember(Name = "timeestimate")]
        public object Timeestimate { get; set; }

        [DataMember(Name = "aggregatetimeoriginalestimate")]
        public object Aggregatetimeoriginalestimate { get; set; }

        [DataMember(Name = "versions")]
        public IList<object> Versions { get; set; }

        [DataMember(Name = "issuelinks")]
        public IList<object> Issuelinks { get; set; }

        [DataMember(Name = "assignee")]
        public Assignee Assignee { get; set; }

        [DataMember(Name = "updated")]
        public DateTime Updated { get; set; }

        [DataMember(Name = "status")]
        public Status Status { get; set; }

        [DataMember(Name = "components")]
        public IList<object> Components { get; set; }

        [DataMember(Name = "timeoriginalestimate")]
        public object Timeoriginalestimate { get; set; }

        [DataMember(Name = "description")]
        public object Description { get; set; }

        [DataMember(Name = "customfield_10010")]
        public object Customfield10010 { get; set; }

        [DataMember(Name = "customfield_10014")]
        public object Customfield10014 { get; set; }

        [DataMember(Name = "customfield_10015")]
        public object Customfield10015 { get; set; }

        [DataMember(Name = "customfield_10005")]
        public object Customfield10005 { get; set; }

        [DataMember(Name = "customfield_10006")]
        public object Customfield10006 { get; set; }

        [DataMember(Name = "customfield_10007")]
        public object Customfield10007 { get; set; }

        [DataMember(Name = "security")]
        public object Security { get; set; }

        [DataMember(Name = "customfield_10008")]
        public object Customfield10008 { get; set; }

        [DataMember(Name = "customfield_10009")]
        public object Customfield10009 { get; set; }

        [DataMember(Name = "aggregatetimeestimate")]
        public object Aggregatetimeestimate { get; set; }

        [DataMember(Name = "summary")]
        public string Summary { get; set; }

        [DataMember(Name = "creator")]
        public Creator Creator { get; set; }

        [DataMember(Name = "subtasks")]
        public IList<object> Subtasks { get; set; }

        [DataMember(Name = "reporter")]
        public Reporter Reporter { get; set; }

        [DataMember(Name = "aggregateprogress")]
        public AggregateProgress Aggregateprogress { get; set; }

        [DataMember(Name = "customfield_10000")]
        public string Customfield10000 { get; set; }

        [DataMember(Name = "customfield_10001")]
        public object Customfield10001 { get; set; }

        [DataMember(Name = "customfield_10002")]
        public object Customfield10002 { get; set; }

        [DataMember(Name = "customfield_10003")]
        public object Customfield10003 { get; set; }

        [DataMember(Name = "customfield_10004")]
        public object Customfield10004 { get; set; }

        [DataMember(Name = "environment")]
        public object Environment { get; set; }

        [DataMember(Name = "duedate")]
        public object Duedate { get; set; }

        [DataMember(Name = "progress")]
        public ProgressItem Progress { get; set; }

        [DataMember(Name = "votes")]
        public VotesItem Votes { get; set; }
    }

    [DataContract]
    public class Issue
    {

        [DataMember(Name = "expand")]
        public string Expand { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "fields")]
        public Fields Fields { get; set; }
    }

    [DataContract]
    public class UserIssues
    {

        [DataMember(Name = "expand")]
        public string Expand { get; set; }

        [DataMember(Name = "startAt")]
        public int StartAt { get; set; }

        [DataMember(Name = "maxResults")]
        public int MaxResults { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }

        [DataMember(Name = "issues")]
        public IList<Issue> Issues { get; set; }
    }

    [DataContract()]
    public class UserInfo
    {

        [DataMember()]
        public string Self { get; set; }

        [DataMember()]
        public string AccountId { get; set; }

        [DataMember()]
        public string AccountType { get; set; }

        [DataMember()]
        public string EmailAddress { get; set; }

        [DataMember()]
        public AvatarUrls AvatarUrls { get; set; }

        [DataMember()]
        public string DisplayName { get; set; }

        [DataMember()]
        public bool Active { get; set; }

        [DataMember()]
        public string TimeZone { get; set; }

        [DataMember()]
        public string Locale { get; set; }
    }

    [DataContract(Name = "avatarUrls")]
    public class AvatarUrls
    {

        [DataMember(Name = "48x48")]
        public string Large { get; set; }

        [DataMember(Name = "24x24")]
        public string Standard { get; set; }

        [DataMember(Name = "16x16")]
        public string Small { get; set; }

        [DataMember(Name = "32x32")]
        public string Medium { get; set; }
    }

    [DataContract]
    public class ContentItem
    {

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }
    }

    [DataContract]
    public class Content
    {

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "content")]
        public IList<ContentItem> ContentItems { get; set; }
    }

    [DataContract]
    public class Body
    {

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "content")]
        public IList<Content> Content { get; set; }
    }

    [DataContract]
    public class UserComment
    {

        [DataMember(Name = "body")]
        public Body Body { get; set; }

        [DataMember(Name = "visibility")]
        public object Visibility { get; set; }
    }

    public static class StartupHelper
    {
        public const string ClientName = "jira";

        public static void AddJiraService(this IServiceCollection services)
        {
            services.AddHttpClient(ClientName, c =>
            {
                c.BaseAddress = new Uri("https://easyscrum.atlassian.net");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "dnN1aG92ODdAZ21haWwuY29tOkEySG5VYTE4YXV5SklINHhOR0w5RDYxRQ==");
            });
            services.AddSingleton<IJiraService, JiraService>();
        }
    }
}

//fdsf