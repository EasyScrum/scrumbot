using System;
using Microsoft.Bot.Schema.Teams;

namespace ScrumBot.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }

        public TeamsChannelAccount TeamsUserInfo { get; set; }
    }
}