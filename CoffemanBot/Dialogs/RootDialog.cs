using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CoffemanBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private string name;
        private int caps;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (string.IsNullOrEmpty(name))
            {
                await this.SendWelcomeMessageAsync(context);
            }
            else
            {
                await SendHowManyCapsDrink(context);
            }
        }

        private async Task SendHowManyCapsDrink(IDialogContext context)
        {
            try
            {
                context.Call(new CapCountDialog(name), CapsCountDialogResumeAfter);
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("Извиняюсь, но есть проблема понимания, попробуй позже");
                await SendWelcomeMessageAsync(context);
            }
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            await context.PostAsync("Привет друг, я Coffeman Bot, буду следить за тобой :)");
            context.Call(new UserNameDialog(), UserNameDialogResumeAfter);
        }

        private async Task UserNameDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            name = await result;
            await SendHowManyCapsDrink(context);
        }

        private async Task CapsCountDialogResumeAfter(IDialogContext context, IAwaitable<int> result)
        {
            try
            {
                caps += await result;
                await context.PostAsync($"{ name } всего выпил { caps } чашек кофе.");
            }
            catch (TooManyAttemptsException)
            {
                await context.PostAsync("Извиняюсь, но есть проблема понимания, попробуй позже");
            }
            finally
            {
                await SendHowManyCapsDrink(context);
            }
        }
    }
}