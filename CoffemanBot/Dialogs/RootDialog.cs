using System;
using System.Threading.Tasks;
using CoffemanBot.Common;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CoffemanBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private bool userWelcomed;

        public Task StartAsync(IDialogContext context)
        {
            int caps;
            if (!context.UserData.TryGetValue(ContextConstants.TotalCaps, out caps))
            {
                caps = 0;
                context.UserData.SetValue(ContextConstants.TotalCaps, caps);
            }

            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string userName;
            if (!context.UserData.TryGetValue(ContextConstants.UserNameKey, out userName))
            {
                PromptDialog.Text(
                    context, 
                    ResumeAfterUserNamePrompt, 
                    "Друг, я тебя не знаю, как мне тебя называть??");
                return;
            }

            int caps = context.UserData.GetValue<int>(ContextConstants.TotalCaps);

            if (!userWelcomed)
            {
                userWelcomed = true;
                await context.PostAsync($"С возвращением о {userName}!");
                context.Wait(MessageReceivedAsync);
                return;
            }

            int capNumberFromMessage;
            if (Int32.TryParse(message.Text, out capNumberFromMessage))
            {
                if (capNumberFromMessage <= 0)
                {
                    await context.PostAsync($"Во во во {userName}! {DialogMessages.BadCapsNumber}");
                }
                else
                {
                    caps += capNumberFromMessage;
                    context.UserData.SetValue(ContextConstants.TotalCaps, caps);
                    await context.PostAsync($"{userName}{DialogMessages.CapsAdded}. Твой счет: {caps}");
                }
            }
            else if (message.Text.Contains("счет"))
            {
                await context.PostAsync($"{userName}. Твой счет: {caps}");
            }
            else
            {
                await context.PostAsync(DialogMessages.TooManyAttemptsMessage);
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task ResumeAfterUserNamePrompt(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var userName = await result;
                userWelcomed = true;

                await context.PostAsync($"{userName} {DialogMessages.WelcomeMessage}");

                context.UserData.SetValue(ContextConstants.UserNameKey, userName);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync(DialogMessages.TooManyAttemptsMessage);
            }

            context.Wait(this.MessageReceivedAsync);
        }
    }
}