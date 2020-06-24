using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScrumBot.Models;

namespace ScrumBot.Contracts
{
    public interface IIssueTrackingIntegrationService
    {
        Task<IEnumerable<UserInfo>> GetUsers();

        Task<IEnumerable<TicketInfo>> GetUserTickets(Guid userId);

        Task<bool> SubmitComment(Guid ticketId, string comment);
    }
}