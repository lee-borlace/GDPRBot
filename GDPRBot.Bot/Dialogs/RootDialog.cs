using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

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

            UpdateBotState(context);

            context.UserData.SetValue("name", "Lee");

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            // return our reply to the user
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }

        private void UpdateBotState(IDialogContext context)
        {
            _messageCount++;

            context.UserData.SetValue("MessageCount", $"Message count in UserData = {_messageCount}");
            context.ConversationData.SetValue("MessageCount", $"Message count in ConversationData = {_messageCount}");
            context.PrivateConversationData.SetValue("MessageCount", $"Message count in PrivateConversationData = {_messageCount}");
        }
    }
}