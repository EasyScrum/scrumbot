using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScrumBot.Contracts;
using ScrumBot.Models;

namespace ScrumBot.Services
{
    public class JiraIntegrationService: IIssueTrackingIntegrationService
    {
        public async Task<IEnumerable<UserInfo>> GetUsers()
        {
            return new List<UserInfo>()
            {
                new UserInfo()
                {
                    Id = "85B9E0E7-0A52-4843-B0BE-819EC84AFF8E",
                    FirstName = "Anton",
                    Lastname = "F",
                    Email = "filisyevav@gmail.com"
                },
                new UserInfo()
                {
                    Id = "FE2DDBE4-C85B-4AD2-A6ED-957689AECAF0",
                    FirstName = "Mikhail",
                    Lastname = "K",
                    Email = "mikhail_k@gmail.com"
                }
            };
        }

        public async Task<IEnumerable<TicketInfo>> GetUserTickets(string userId)
        {
            return GetTickets().Where(x => x.AssigneeId == userId).ToList();
        }

        public async Task<IEnumerable<TicketInfo>> GetTicketsByUserEmail(string userEmail)
        {
            var users = await GetUsers();
            var user = users.FirstOrDefault(x => string.Equals(x.Email, userEmail, StringComparison.InvariantCultureIgnoreCase));

            if (user == null) return new List<TicketInfo>();

            return await GetUserTickets(user.Id);
        }

        private List<TicketInfo> GetTickets()
        {
            return new List<TicketInfo>()
            {
                new TicketInfo()
                {
                    Id = "B6B34D6E-1921-40EA-BA38-A714578E512D",
                    Name = "ESM-2",
                    Title = "Test ticket title for esm-2",
                    AssigneeId = "85B9E0E7-0A52-4843-B0BE-819EC84AFF8E"
                },
                new TicketInfo()
                {
                    Id = "588E32DB-250E-41BB-98CD-44007F691318",
                    Name = "ESM-5",
                    Title = "Test ticket title for esm-5",
                    AssigneeId = "85B9E0E7-0A52-4843-B0BE-819EC84AFF8E"
                },
                new TicketInfo()
                {
                    Id = "46ED5378-F74D-4EFE-9C06-C61A4687ED3B",
                    Name = "ESM-1",
                    Title = "Test ticket title for esm-1",
                    AssigneeId = "FE2DDBE4-C85B-4AD2-A6ED-957689AECAF0"
                },
                new TicketInfo()
                {
                    Id = "C47EA4B0-B203-4EE7-997A-9363BC3CA41D",
                    Name = "ESM-3",
                    Title = "Test ticket title for esm-3",
                    AssigneeId = "FE2DDBE4-C85B-4AD2-A6ED-957689AECAF0"
                }
            };
        }

        public async Task<bool> SubmitComment(string ticketId, string comment)
        {
            return true;
        }
    }
}
