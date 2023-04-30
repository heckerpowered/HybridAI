using System;
using System.Linq;

namespace HybridAI.Security
{
    internal record EncryptionDescriptor(byte[] EncryptionKey, byte[] InitializationVector)
    {
        public static EncryptionDescriptor GetEncryptionDescriptor()
        {
            var credential = EncryptionManager.GetCredential();
            return GetEncryptionDescriptor(credential);
        }

        public static EncryptionDescriptor GetEncryptionDescriptor(byte[] credential)
        {
            var encryptionKey = EncryptionManager.GetEncryptionKey(credential);
            var initializationVector = EncryptionManager.GetInitializationVector(credential);

            return new(encryptionKey, initializationVector);
        }

        public virtual bool Equals(EncryptionDescriptor? encryptionDescriptor)
        {
            if (encryptionDescriptor == null)
            {
                return false;
            }

            return encryptionDescriptor.EncryptionKey.SequenceEqual(EncryptionKey)
                && encryptionDescriptor.InitializationVector.SequenceEqual(InitializationVector);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EncryptionKey.GetHashCode(), InitializationVector.GetHashCode());
        }
    }
}
