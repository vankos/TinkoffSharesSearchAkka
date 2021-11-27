using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffSearchLib.Messages
{
    public class TextMessage
    {
        public TextMessage(string messageText, bool showNotification)
        {
            MessageText = messageText;
            ShowNotification = showNotification;
        }

        public string MessageText { get; set; }

        public bool ShowNotification { get; set; }
    }
}
