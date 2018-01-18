using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CoffemanBot.Dialogs
{
    [Obsolete]
    [Serializable]
    public class UserNameDialog : IDialog<string>
    {
        private int attempts = 3;
        private string name;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Друг, я тебя не знаю, как мне тебя называть?");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            if ((activity.Text != null) && (activity.Text.Trim().Length > 0))
            {
                name = activity.Text.Trim();
                PromptDialog.Confirm(
                context,
                AfterConfirmAsync,
                $"Я буду называть тебя {activity.Text.Trim()}?",
                "Didn't get that!",
                promptStyle: PromptStyle.None);
            }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("Дружище, Я не понял тебя. Как тебя называть (т.е. 'Маша', 'Властелин')?");
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a string or was an empty string."));
                }
            }
        }

        public async Task AfterConfirmAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                context.Done(name);
            }
            else
            {
                await context.PostAsync("Дружище, не грусти, напиши мне новое имя");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}