using System.Collections.Generic;

namespace ScrumBot.Models
{
    public class UsersReviewDialogOptions
    {
        public List<UserDetails> Users { get; set; }
        public List<string> ReportedUsers { get; set; }
    }
}