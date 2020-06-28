using System;
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
        public static Activity GetMessageActivityWithMention(UserInfo user, string textPattern)
        {
            if (Settings.UseTeams)
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
    }
}
