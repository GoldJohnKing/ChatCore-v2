using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace ChatCore.Utilities
{
	public static class CryptoUtils
	{
		public static string encryptString(string plainText, string key, string IV)
		{
			if (!string.IsNullOrEmpty(plainText) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(IV))
			{
				return System.Text.Encoding.UTF8.GetString(EncryptStringToBytes_Aes(plainText, System.Text.Encoding.UTF8.GetBytes(key), System.Text.Encoding.UTF8.GetBytes(IV)));
			}
			

			return string.Empty;
		}

		public static string decryptString(string cipherText, string key, string IV)
		{
			if (!string.IsNullOrEmpty(cipherText) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(IV))
			{
				return DecryptStringFromBytes_Aes(System.Text.Encoding.UTF8.GetBytes(cipherText), System.Text.Encoding.UTF8.GetBytes(key), System.Text.Encoding.UTF8.GetBytes(IV));
			}

			return string.Empty;
		}

		// ref: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netstandard-2.1
		private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
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
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							//Write all data to the stream.
							swEncrypt.Write(plainText);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}

			// Return the encrypted bytes from the memory stream.
			return encrypted;
		}

		private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
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
				using (MemoryStream msDecrypt = new MemoryStream(cipherText))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{

							// Read the decrypted bytes from the decrypting stream
							// and place them in a string.
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}

			return plaintext;
		}
	}
}