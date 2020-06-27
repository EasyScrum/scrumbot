﻿using System.Collections.Generic;

namespace ScrumBot.Models
{
    public class TicketsReviewDialogOptions
    {
        public UserInfo User { get; set; }
        public List<TicketInfo> Tickets { get; set; }
        public List<string> ReportedTickets { get; set; }
    }

    public class TicketStatusDialogOptions
    {
        public UserInfo User { get; set; }
        public TicketInfo Ticket { get; set; }
    }
}