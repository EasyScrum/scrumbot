using System;
using System.IO;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ScrumBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private Templates _templates;
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, IStorage storage,
            UserState userState, ConversationState conversationState, IConfiguration configuration)
            : base(credentialProvider)
        {
            this.UseStorage(storage);
            this.UseBotState(userState);
            this.UseBotState(conversationState);

            string[] paths = { ".", $"{nameof(AdapterWithErrorHandler)}.lg" };
            string fullPath = Path.Combine(paths);
            _templates = Templates.ParseFile(fullPath);

            OnTurnError = async (turnContext, exception) =>
            {
                logger.LogError($"Exception caught : {exception.Message}");

                await turnContext.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate("SomethingWentWrong", exception)));

                if (conversationState != null)
                {
                    try
                    {
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}
