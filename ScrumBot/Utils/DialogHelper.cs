﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using ScrumBot.Models;

namespace ScrumBot.Utils
{
    public static class DialogHelper
    {
        public static Activity GetMessageActivityWithMention(UserDetails user, string textPattern, bool useTeams)
        {
            if (useTeams)
            {
                var mention = new Mention
                {
                    Mentioned = user.TeamsUserInfo,
                    Text = $"<at>{XmlConvert.EncodeName(user.TeamsUserInfo.GivenName)}</at>"
                };

                var replyActivity = MessageFactory.Text(string.Format(textPattern, mention.Text));
                replyActivity.Entities = new List<Entity> { mention };

                return replyActivity;
            }
            else
            {
                return MessageFactory.Text(string.Format(textPattern, user.FirstName));
            }
        }

        public static string GetUserFullName(UserDetails user)
        {
            return user.TeamsUserInfo?.GivenName ?? user.FirstName;
        }

        public static bool IsExpectedUser(UserDetails user, ChannelAccount fromUser, bool useTeams)
        {
            if (useTeams && user.TeamsUserInfo != null)
            {
                return string.Equals(user.TeamsUserInfo.Id, fromUser.Id, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return true;
            }
        }
    }
}
