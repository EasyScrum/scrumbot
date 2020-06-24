using System;

namespace ScrumBot.Models
{
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
    }
}