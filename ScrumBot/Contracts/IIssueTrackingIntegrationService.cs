using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScrumBot.Models;

namespace ScrumBot.Contracts
{
    public interface IIssueTrackingIntegrationService
    {
        Task<IEnumerable<UserInfo>> GetUsers();

        Task<IEnumerable<TicketInfo>> GetUserTickets(string userId);
        Task<IEnumerable<TicketInfo>> GetTicketsByUserEmail(string userEmail);

        Task<bool> SubmitComment(string ticketId, string comment);
    }
}