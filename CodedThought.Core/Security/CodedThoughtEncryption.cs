using System.Security.Cryptography;

namespace CodedThought.Core.Security {

	public class CodedThoughtEncryption {
		private static string clearkey = "mmdjkmhoppoldkimjhlghyhejufincxp";//key size 256bit (aes requires min 128bit max 256bit)

		/// <summary>Encrypts the password.</summary>
		/// <param name="clearval">The clearval.</param>
		/// <returns></returns>
		public static string EncryptPassword(string clearval) {
			MemoryStream ms = null;
			CryptoStream cs = null;
			string EncryptedPassword = string.Empty;

			try {
				byte[] clearTextBytes = Encoding.UTF8.GetBytes(clearval);
				byte[] rgbIV = Encoding.ASCII.GetBytes("ryojvlzmdalyglrj");
				byte[] key = Encoding.ASCII.GetBytes(clearkey);

				SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();

				ms = new MemoryStream();
				cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIV), CryptoStreamMode.Write);
				cs.Write(clearTextBytes, 0, clearTextBytes.Length);

				cs.Close(); //must close here else cannot work
				ms.Close();

				EncryptedPassword = Convert.ToBase64String(ms.ToArray());
			} catch (Exception ex) {
				throw ex;
			} finally {
				if (cs != null)
					cs.Close();
				if (ms != null)
					ms.Close();
			}

			return EncryptedPassword;
		}

		/// <summary>Decrypts the password.</summary>
		/// <param name="encrypVal">The encryp val.</param>
		/// <returns></returns>
		public static string DecryptPassword(string encrypVal) {
			string DecryptedPassword = string.Empty;
			MemoryStream ms = null;
			CryptoStream cs = null;

			try {
				byte[] encryptedTextBytes = Convert.FromBase64String(encrypVal);
				byte[] rgbIV = Encoding.ASCII.GetBytes("ryojvlzmdalyglrj");
				byte[] key = Encoding.ASCII.GetBytes(clearkey);

				ms = new MemoryStream();

				SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();

				cs = new CryptoStream(ms, rijn.CreateDecryptor(key, rgbIV), CryptoStreamMode.Write);

				cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);

				cs.Close();//must close here else cannot work
				ms.Close();

				DecryptedPassword = Encoding.UTF8.GetString(ms.ToArray());
			} catch (Exception ex) {
				throw ex;
			} finally {
				if (cs != null)
					cs.Close();
				if (ms != null)
					ms.Close();
			}

			return DecryptedPassword;
		}
	}
}