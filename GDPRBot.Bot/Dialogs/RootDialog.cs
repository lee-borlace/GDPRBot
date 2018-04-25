using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;

namespace GDPRBot.Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private int _messageCount = 0;


        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Interesting, this correctly uses the storage configured in Global.asax.cs and doesn't generate the warning in the emulator. However
            // StateClient and GetUserDataAsync() are both obsolete. How are we supposed to get these?
            StateClient sc = context.Activity.GetStateClient();
            var userData = await sc.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            userData.SetProperty("name", "Lee");

            UpdateBotState(context);

            switch (activity.Text.ToLower())
            {
                // TODO : Other new methods exposed for GDPR.
                case "export":
                    await DoExport(context);
                    break;
                default:
                    {
                        // calculate something for us to return
                        int length = (activity.Text ?? string.Empty).Length;

                        // return our reply to the user
                        await context.PostAsync($"You sent {activity.Text} which was {length} characters");

                        context.Wait(MessageReceivedAsync);
                    }
                    break;
            }


           
        }

        private async Task DoExport(IDialogContext context)
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, context.Activity.AsMessageActivity()))
            {
                // TODO : This doesn't work. How the dickens do we get an IBotState so we can call ExportBotStateDataAsync() on it? 
                var botState = scope.Resolve<IBotState>();

                var result = await botState.ExportBotStateDataAsync("emulator");

                await context.PostAsync(result.BotStateData.Count.ToString());
                context.Wait(MessageReceivedAsync);
            }
        }

        /// <summary>
        /// Write some bot state.
        /// </summary>
        /// <param name="context">The context.</param>
        private void UpdateBotState(IDialogContext context)
        {
            _messageCount++;

            context.UserData.SetValue("MessageCount", $"Message count in UserData = {_messageCount}");
            context.ConversationData.SetValue("MessageCount", $"Message count in ConversationData = {_messageCount}");
            context.PrivateConversationData.SetValue("MessageCount", $"Message count in PrivateConversationData = {_messageCount}");
        }
    }
}