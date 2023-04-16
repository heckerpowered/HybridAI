using System.Security.Cryptography;
using System.Text;

namespace Aes_Example
{
    class AesExample
    {
        public static int KeySize { get; private set; } = 32;
        public static int IVSize { get; private set; } = 16;

        private static byte[] GetCredential()
        {
            return SHA512.HashData(Encoding.UTF8.GetBytes(Environment.MachineName));
        }

        private static byte[] GeKey(byte[] credential)
        {
            if (credential.Length == KeySize)
            {
                return credential;
            }
            else if (credential.Length < KeySize)
            {
                using var memoryStream = new MemoryStream(KeySize);

                var times = KeySize / credential.Length + 1;
                for (int i = 0; i < times; ++i)
                {
                    memoryStream.Write(credential);
                }

                memoryStream.SetLength(KeySize);
                return memoryStream.ToArray();
            }
            else
            {
                return credential.Take(KeySize).ToArray();
            }
        }

        private static byte[] GetIV(byte[] credential)
        {
            if (credential.Length == IVSize)
            {
                return credential;
            }
            else if (credential.Length < IVSize)
            {
                using var memoryStream = new MemoryStream(IVSize);

                var times = IVSize / credential.Length + 1;
                for (int i = 0; i < times; ++i)
                {
                    memoryStream.Write(credential);
                }

                memoryStream.SetLength(IVSize);
                return memoryStream.ToArray();
            }
            else
            {
                return credential.Take(IVSize).ToArray();
            }
        }

        public static void Main()
        {
            string original = "Here is some data to encrypt!";

            // Create a new instance of the Aes
            // class.  This generates a new key and initialization
            // vector (IV).
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = GeKey(GetCredential());
                myAes.IV = GetIV(GetCredential());

                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

                Aes tmp = Aes.Create();
                tmp.Key = GeKey(GetCredential());
                tmp.IV = GetIV(GetCredential());

                // Decrypt the bytes to a string.
                string roundtrip = DecryptStringFromBytes_Aes(encrypted, tmp.Key, myAes.IV);

                //Display the original data and the decrypted data.
                Console.WriteLine("Original:   {0}", original);
                Console.WriteLine("Round Trip: {0}", roundtrip);
            }
        }
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using var msEncrypt = File.Create("text.txt");
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                }

                using var fs = File.Open("text.txt", FileMode.Open);
                encrypted = Encoding.UTF8.GetBytes(new StreamReader(fs).ReadToEnd());
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using var msDecrypt = File.Open("text.txt", FileMode.Open);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }
    }
}