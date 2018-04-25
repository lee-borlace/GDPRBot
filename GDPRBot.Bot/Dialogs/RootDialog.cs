using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System.Threading;

namespace GDPRBot.Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private int _messageCount = 0;

        private const string StateKey = "MessageCount";

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            //// Interesting, this correctly uses the storage configured in Global.asax.cs and doesn't generate the warning in the emulator. However
            //// StateClient and GetUserDataAsync() are both obsolete. How are we supposed to get these?
            //StateClient sc = context.Activity.GetStateClient();
            //var userData = await sc.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            //userData.SetProperty("name", "Lee");

            

            switch (activity.Text.ToLower())
            {
                // TODO : Other new methods exposed for GDPR.
                case "show":
                    await ShowBotState(context);
                    break;
                case "export":
                    await DoExport(context);
                    break;
                case "clear":
                    await DoClear(context);
                    break;
                default:
                    {
                        UpdateBotState(context);

                        // calculate something for us to return
                        int length = (activity.Text ?? string.Empty).Length;

                        // return our reply to the user
                        await context.PostAsync($"You sent {activity.Text} which was {length} characters");

                        context.Wait(MessageReceivedAsync);
                    }
                    break;
            }



        }

        private async Task ShowBotState(IDialogContext context)
        {
            await context.PostAsync("Bot state as follows for 'MessageCount' key :");

            string temp;

            var value = context.UserData.TryGetValue(StateKey, out temp) ? temp : "N/A";
            await context.PostAsync($"UserData : {value}");

            value = context.PrivateConversationData.TryGetValue(StateKey, out temp) ? temp : "N/A";
            await context.PostAsync($"PrivateConversationData : {value}");
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


        private async Task DoClear(IDialogContext context)
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, context.Activity.AsMessageActivity()))
            {
                var dataStore = scope.Resolve<IBotDataStore<BotData>>();

                var address = new Address(
                   context.Activity.Recipient.Id,
                   context.Activity.ChannelId,
                   context.Activity.From.Id,
                   context.Activity.Conversation.Id,
                   context.Activity.ServiceUrl);

                var token = default(CancellationToken);

                await dataStore.SaveAsync(address, BotStoreType.BotUserData, null, token);
                await dataStore.SaveAsync(address, BotStoreType.BotPrivateConversationData, null, token);
                await dataStore.FlushAsync(address, token);
            }

            // This works but won't be of much use outside the context of the bot.
            //context.UserData.Clear();
            //context.PrivateConversationData.Clear();

            await context.PostAsync("All cleared!");
        }


        /// <summary>
        /// Write some bot state.
        /// </summary>
        /// <param name="context">The context.</param>
        private void UpdateBotState(IDialogContext context)
        {
            _messageCount++;

            context.UserData.SetValue(StateKey, $"Message count in UserData = {_messageCount}");
            context.PrivateConversationData.SetValue(StateKey, $"Message count in PrivateConversationData = {_messageCount}");
        }
    }
}