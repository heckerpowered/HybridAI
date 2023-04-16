namespace HybridAI.Control.Chat
{
    /// <summary>
    /// The class of the message, used to distinguish whether the message accepts retries or cancels requests.
    /// Usually responses that are not fully accepted can be cancelled or retried. Error messages can also be retried,
    /// where the user input does not support any action.
    /// </summary>
    public enum MessageKind
    {
        /// <summary>
        /// User input, which does not process any retry or cancel requests.
        /// </summary>
        UserMessage,

        /// <summary>
        /// Response from AI, such messages can be retried and cancelled.
        /// </summary>
        ResponseMessage,

        /// <summary>
        /// An exception was encountered during the request, and such messages can be retried.
        /// </summary>
        ErrorMessage
    }
}
