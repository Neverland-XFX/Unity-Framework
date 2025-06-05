using System;

namespace UnityFramework
{
    public class MessageBase : EventArgs
    {
        public MessageBase(object sender)
        {
            this.Sender = sender;
        }

        /// <summary>
        /// Gets or sets the message's sender.
        /// </summary>
        public object Sender { get; protected set; }
    }
}