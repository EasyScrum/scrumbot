using System.Collections.Generic;
using System.Threading.Tasks;
using ScrumBot.Models;

namespace ScrumBot.Contracts
{
    public interface IJiraService
    {
        Task<IEnumerable<UserInfo>> GetUsersAsync(string projectId);
        Task<UserIssues> GetIssuesAsync(string assigneeId, string status);
        Task<bool> TryAddCommentAsync(string issueId, UserComment comment);
    }
}