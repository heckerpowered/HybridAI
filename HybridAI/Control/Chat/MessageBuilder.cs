﻿using System.Threading;
using System.Windows.Media;

namespace HybridAI.Control.Chat
{
    public class MessageBuilder
    {
        internal string text = string.Empty;
        internal bool performAnimation = false;
        internal Brush? foreground;
        internal MessageKind kind;
        internal CancellationTokenSource? cancellationTokenSource;
        internal MainWindow? container;

        /// <summary>
        /// Set the message to be displayed
        /// </summary>
        /// <param name="text">the message to be displayed</param>
        /// <returns>Current modified instance</returns>
        public MessageBuilder SetText(string text)
        {
            this.text = text;
            return this;
        }

        /// <summary>
        /// Causes an animation to be played for each word of the message when the message control is rendered.
        /// If not set, the message will have an animation for the whole. If set so that each character plays an animation,
        /// the message acceptance process will be delayed by the message.
        /// </summary>
        /// <returns>Current modified instance</returns>
        public MessageBuilder SetPlayAnimation()
        {
            performAnimation = true;
            return this;
        }

        /// <summary>
        /// Set the message font color, typically used to distinguish between user messages (black or default color),
        /// responses (blue-violet gradient), and error messages (red).
        /// </summary>
        /// <param name="foreground">Message font color</param>
        /// <returns>Current modified instance</returns>
        public MessageBuilder SetForeground(Brush foreground)
        {
            this.foreground = foreground;
            return this;
        }

        /// <summary>
        /// Set the kind of message, which affects whether it can be retried or cancelled.
        /// </summary>
        /// <param name="kind">The kind of the message</param>
        /// <returns>Current modified instance</returns>
        public MessageBuilder SetMessageKind(MessageKind kind)
        {
            this.kind = kind;
            return this;
        }

        /// <summary>
        /// Set the cancellation token source for the message, which is used to cancel the request to AI.
        /// </summary>
        /// <param name="cancellationTokenSource">Cancellation token source</param>
        /// <returns>Current modified instance</returns>
        public MessageBuilder SetCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
            return this;
        }

        /// <summary>
        /// Set the mainWindow of the message, which is the message in.
        /// </summary>
        /// <param name="container">Container item collection</param>
        /// <returns>Current modified instance</returns>
        public MessageBuilder SetContainer(MainWindow container)
        {
            this.container = container;
            return this;
        }
    }
}
