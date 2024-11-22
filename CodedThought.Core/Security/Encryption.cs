using System.Security.Cryptography;

namespace CodedThought.Core.Security
{

    /// <summary>Summary description for Encryption.</summary>
    public class Encryption
    {
        public Encryption()
        {
            // TODO: Add constructor logic here
        }

        #region Methods

        /// <summary>Decrypts the file.</summary>
        /// <param name="file">           The file.</param>
        /// <param name="FileDestination">The file destination.</param>
        /// <param name="Password">       The password.</param>
        public static void DecryptFile(FileStream file, string FileDestination, string Password)
        {
            FileStream fsIn = file;
            FileStream fsOut = new(FileDestination, FileMode.Create, FileAccess.Write);

            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            PasswordDeriveBytes SecretKey = new(Password, Salt);
            using Aes aesCipher = Aes.Create();
            ICryptoTransform Decryptor = aesCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            using CryptoStream cryptoStream = new(fsIn, Decryptor, CryptoStreamMode.Read);
            int ByteData;
            while ((ByteData = cryptoStream.ReadByte()) != -1)
            {
                fsOut.WriteByte((byte) ByteData);
            }
            cryptoStream.Close();
            fsIn.Close();
            fsOut.Close();
        }

        /// <summary>Decrypts the specified p value.</summary>
        /// <param name="pValue">The p value.</param>
        /// <returns>System.String.</returns>
        public static string DecryptMD5(string pValue)
        {
            if (pValue == string.Empty)
            {
                return "";
            }
            byte[] array = Convert.FromBase64String(pValue);
            MD5 md5 = MD5.Create();
            byte[] key = md5.ComputeHash(Encoding.UTF8.GetBytes("9876543210a7ABbC3cDd7EeFfGg4H8hIiJj1KkLlM8m—91ÒOoPp3QqRrSs4TtUuVvWwXxY5yZ9z0123456789"));
            md5.Clear();

            TripleDES tripleDES = TripleDES.Create();
            tripleDES.Key = key;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = tripleDES.CreateDecryptor();
            byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
            tripleDES.Clear();
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>Decrypts the string.</summary>
        /// <param name="InputText">The input text.</param>
        /// <param name="Password"> The password.</param>
        /// <returns></returns>
        public static string DecryptString(string input, string key)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input text cannot be null or empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty");

            byte[] cipherBytes = Convert.FromBase64String(input);

            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(key);
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(cipherBytes, iv, iv.Length);
                aes.IV = iv;

                using MemoryStream memoryStream = new(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
                using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using StreamReader reader = new(cryptoStream);
                return reader.ReadToEnd();
            }
        }

        /// <summary>Encrypts the file.</summary>
        /// <param name="FileLocation">   The file location.</param>
        /// <param name="FileDestination">The file destination.</param>
        /// <param name="Password">       The password.</param>
        public static void EncryptFile(string FileLocation, string FileDestination, string Password)
        {
            // First we are going to open the file streams
            FileStream fsIn = new(FileLocation, FileMode.Open, FileAccess.Read);
            FileStream fsOut = new(FileDestination, FileMode.Create, FileAccess.Write);

            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            PasswordDeriveBytes SecretKey = new(Password, Salt);
            using Aes aesCipher = Aes.Create();
            ICryptoTransform Encryptor = aesCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            using CryptoStream csEncrypt = new(fsOut, Encryptor, CryptoStreamMode.Write);
            int ByteData;
            while ((ByteData = fsIn.ReadByte()) != -1)
            {
                csEncrypt.WriteByte((byte) ByteData);
            }
            csEncrypt.Close();
            fsIn.Close();
            fsOut.Close();
        }

        public static void EncryptFileFromStream(FileStream file, string FileDestination, string Password)
        {
            // First we are going to open the file streams
            FileStream fsIn = file;
            FileStream fsOut = new(FileDestination, FileMode.Create, FileAccess.Write);

            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            PasswordDeriveBytes SecretKey = new(Password, Salt);
            using Aes aesCipher = Aes.Create();
            ICryptoTransform Encryptor = aesCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            using CryptoStream csEncrypt = new(fsOut, Encryptor, CryptoStreamMode.Write);
            int ByteData;
            while ((ByteData = fsIn.ReadByte()) != -1)
            {
                csEncrypt.WriteByte((byte) ByteData);
            }
            csEncrypt.Close();
            fsIn.Close();
            fsOut.Close();
        }

        /// <summary>Encrypts the specified p value.</summary>
        /// <param name="pValue">The p value.</param>
        /// <returns>System.String.</returns>
        public static string EncryptMD5(string pValue)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(pValue);

            MD5 md5 = MD5.Create();
            byte[] key = md5.ComputeHash(Encoding.UTF8.GetBytes("9876543210a7ABbC3cDd7EeFfGg4H8hIiJj1KkLlM8m—91ÒOoPp3QqRrSs4TtUuVvWwXxY5yZ9z0123456789"));
            md5.Clear();

            TripleDES tripleDES = TripleDES.Create();
            tripleDES.Key = key;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = tripleDES.CreateDecryptor();
            byte[] array = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(array, 0, array.Length);
        }

        /// <summary>Encrypts the string.</summary>
        /// <param name="InputText">The input text.</param>
        /// <param name="Password"> The password.</param>
        /// <returns></returns>
        public static string EncryptString(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be null or empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty");

            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(key);
                aes.GenerateIV();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to ciphertext
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Generates an encryption safe key sized to the <see cref="int">key size</see> passed.
        /// </summary>
        /// <param name="keySize"></param>
        /// <returns></returns>
        public static byte[] GenerateKey(int keySize)
        {
            byte[] randomBytes = new byte[keySize];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return GenerateKey(Encoding.UTF8.GetString(randomBytes, 0, randomBytes.Length));
        }

        /// <summary>
        /// Generates an encryption safe key based on the seed key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static byte[] GenerateKey(string key) => SHA256.HashData(Encoding.UTF8.GetBytes(key));

        #endregion Methods
    }
}