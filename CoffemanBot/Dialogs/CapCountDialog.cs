using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CoffemanBot.Dialogs
{
    [Serializable]
    public class CapCountDialog : IDialog<int>
    {
        private int _attempts = 3;
        private string _name;

        public CapCountDialog(string name)
        {
            _name = name;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"{ _name }, сколько ты выпил?");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result;
            int caps;
            if (Int32.TryParse(activity.Text, out caps) && (caps > 0))
            {
                context.Done(caps);
            }
            else
            {
                --_attempts;
                if (_attempts > 0)
                {
                    await context.PostAsync("Я извиняюсь, но я не понял. Сколько выпил чашек (т.е. '1' или '2')?");
                    context.Wait(this.MessageReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a valid caps."));
                }
            }
        }
    }
}