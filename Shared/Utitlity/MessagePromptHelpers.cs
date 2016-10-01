using Coding4Fun.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Shared.Utitlity
{
    public class MessagePromptHelpers
    {
        public static MessagePrompt getMessagePrompt(string title, string message)
        {
            MessagePrompt prompt = new MessagePrompt();
            prompt.Title = title;//"Missing organisation";
            prompt.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            prompt.Message = message;//"You cannot create a board since you do not belong to any organisation.Please create an organisation and then create a board within that organisation!";
            prompt.IsOverlayApplied = true;
            return prompt;
        }
    }
}
