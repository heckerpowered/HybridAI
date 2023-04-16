namespace HybridAI.History
{
    internal record EncryptionDescriptor(byte[] EncryptionKey, byte[] InitializationVector)
    {
        public static EncryptionDescriptor GetEncryptionDescriptor()
        {
            var credential = ChatHistory.GetCredential();
            var encryptionKey = ChatHistory.GetEncryptionKey(credential);
            var initializationVector = ChatHistory.GetInitializationVector(credential);

            return new(encryptionKey, initializationVector);
        }
    };
}
