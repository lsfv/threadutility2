using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Function
{
    public class IncEncrypt
    {
        private static string g_strTimeStampFormat = "yyyyMMddHHmmssffff";
        private static string g_strDESIV = ComputeMD5HashHEX("2011hrms");

        #region Encrypt/Decrypt

        public static string GeneralEncrypt(string Message)
        {
            string p_strTimeStamp = System.DateTime.Now.ToString(g_strTimeStampFormat);
            p_strTimeStamp = p_strTimeStamp + "|" + Message;
            string p_strMainKey = ComputeMD5HashHEX(p_strTimeStamp);
            string p_strConvertedStamp = p_strMainKey.Substring(9, 1) + p_strMainKey.Substring(10, 8) + DESEncrypt((p_strMainKey.Substring(10, 7) + p_strMainKey.Substring(9, 1)), p_strTimeStamp);
            p_strConvertedStamp = p_strConvertedStamp.Replace("+", "|");
            return p_strConvertedStamp;
        }

        public static string GeneralDecrypt(string Message)
        {
            try
            {
                if (Message == string.Empty)
                    return string.Empty;

                string p_strToken = Message;
                p_strToken = p_strToken.Replace("|", "+");
                //string p_strMainKey = p_strToken.Substring(1, 8);
                string p_strMainKey = p_strToken.Substring(1, 7) + p_strToken.Substring(0, 1);
                string p_strCipher = p_strToken.Substring(9);
                //Response.Write("Key: " + p_strMainKey + System.Environment.NewLine);
                string p_strTimeStamp = DESDecrypt(p_strMainKey, p_strCipher);

                if (p_strTimeStamp == string.Empty)
                {
                    throw new System.Exception("Error in decryption!!");
                }

                string[] p_strDecryptedArray = p_strTimeStamp.Split('|');

                p_strTimeStamp = p_strDecryptedArray[0];
                string p_strReturnMessage = p_strDecryptedArray[1];

                return p_strReturnMessage;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private static string ComputeMD5Hash(string Seed)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider p_MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return System.Convert.ToBase64String(p_MD5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Seed)));
        }
        public static string ComputeMD5HashHEX(string Seed)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider p_MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return ConvertByteArrayToHex(p_MD5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Seed)));
        }
        public static string ConvertByteArrayToHex(byte[] ByteArray)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < ByteArray.Length; i++)
            {
                sb.Append(ByteArray[i].ToString("X2"));
            }
            return sb.ToString();
        }
        private static byte[] ConvertHexToByteArray(string HexString)
        {
            List<byte> p_lstReturnList = new List<byte>();
            for (int i = 0; i < HexString.Length; i = i + 2)
            {
                p_lstReturnList.Add(byte.Parse(HexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
            }
            return p_lstReturnList.ToArray();
        }

        private static string DESEncrypt(string MainKey, string strPlain)
        {
            string g_strDESKey = ComputeMD5HashHEX(MainKey + g_strDESIV).Substring(0, 8);
            string p_strReturn = string.Empty;
            try
            {
                string strDESKey = g_strDESKey;
                string strDESIV = g_strDESIV;
                byte[] bytesDESKey = System.Text.ASCIIEncoding.ASCII.GetBytes(strDESKey);
                byte[] bytesDESIV = System.Text.ASCIIEncoding.ASCII.GetBytes(strDESIV);
                System.Security.Cryptography.DESCryptoServiceProvider desEncrypt = new System.Security.Cryptography.DESCryptoServiceProvider();
                System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream();
                System.Security.Cryptography.CryptoStream csEncrypt = new System.Security.Cryptography.CryptoStream(msEncrypt, desEncrypt.CreateEncryptor(bytesDESKey, bytesDESIV), System.Security.Cryptography.CryptoStreamMode.Write);
                System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt);
                swEncrypt.WriteLine(strPlain);
                swEncrypt.Close();
                csEncrypt.Close();
                byte[] bytesCipher = msEncrypt.ToArray();
                msEncrypt.Close();
                p_strReturn = ConvertByteArrayToHex(bytesCipher);
            }
            catch (System.Exception) { }
            return p_strReturn;
        }
        private static string DESDecrypt(string MainKey, string strCipher)
        {
            string g_strDESKey = ComputeMD5HashHEX(MainKey + g_strDESIV).Substring(0, 8);
            string strPlainText = string.Empty;
            try
            {
                string strDESKey = g_strDESKey;
                string strDESIV = g_strDESIV;
                byte[] bytesDESKey = System.Text.ASCIIEncoding.ASCII.GetBytes(strDESKey);
                byte[] bytesDESIV = System.Text.ASCIIEncoding.ASCII.GetBytes(strDESIV);
                byte[] bytesCipher = ConvertHexToByteArray(strCipher);
                System.Security.Cryptography.DESCryptoServiceProvider desDecrypt = new System.Security.Cryptography.DESCryptoServiceProvider();
                System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(bytesCipher);
                System.Security.Cryptography.CryptoStream csDecrypt = new System.Security.Cryptography.CryptoStream(msDecrypt, desDecrypt.CreateDecryptor(bytesDESKey, bytesDESIV), System.Security.Cryptography.CryptoStreamMode.Read);
                System.IO.StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt);
                strPlainText = srDecrypt.ReadLine();
                srDecrypt.Close();
                csDecrypt.Close();
                msDecrypt.Close();
            }
            catch (System.Exception) { }
            return strPlainText;
        }
        #endregion


        public static string EncryptTextToMemory(string Data, byte[] Key)
        {
            try
            {
                byte[] IV = System.Text.Encoding.UTF8.GetBytes(DESEncrypt(g_strDESIV, System.Text.Encoding.UTF8.GetString(Key)).Substring(0, 24));
                // Create a MemoryStream.
                System.IO.MemoryStream mStream = new System.IO.MemoryStream();

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                CryptoStream cStream = new CryptoStream(mStream,
                    new TripleDESCryptoServiceProvider().CreateEncryptor(Key, IV),
                    CryptoStreamMode.Write);

                // Convert the passed string to a byte array.
                byte[] toEncrypt = new ASCIIEncoding().GetBytes(Data);

                // Write the byte array to the crypto stream and flush it.
                cStream.Write(toEncrypt, 0, toEncrypt.Length);
                cStream.FlushFinalBlock();

                // Get an array of bytes from the 
                // MemoryStream that holds the 
                // encrypted data.
                byte[] ret = mStream.ToArray();

                // Close the streams.
                cStream.Close();
                mStream.Close();

                // Return the encrypted buffer.
                return ConvertByteArrayToHex(ret);
            }
            catch
            {
                //hide the exception
                return string.Empty;
            }

        }

        public static string DecryptTextFromMemory(string Cipher, byte[] Key)
        {
            try
            {
                byte[] IV = System.Text.Encoding.UTF8.GetBytes(DESEncrypt(g_strDESIV, System.Text.Encoding.UTF8.GetString(Key)).Substring(0, 24));
                byte[] Data = ConvertHexToByteArray(Cipher);
                // Create a new MemoryStream using the passed 
                // array of encrypted data.
                System.IO.MemoryStream msDecrypt = new System.IO.MemoryStream(Data);

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                    new TripleDESCryptoServiceProvider().CreateDecryptor(Key, IV),
                    CryptoStreamMode.Read);

                // Create buffer to hold the decrypted data.
                byte[] fromEncrypt = new byte[Data.Length];

                // Read the decrypted data out of the crypto stream
                // and place it into the temporary buffer.
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                //Convert the buffer into a string and return it.
                return new UTF8Encoding().GetString(fromEncrypt);
            }
            catch (System.Exception ex)
            {
                //hide the exception
                return "";
            }
        }


        private static System.Collections.Specialized.NameValueCollection g_Config = System.Configuration.ConfigurationManager.AppSettings;

        public static string GetDBConnectionString(string connstr,string uid,string psw)
        {

            string l_strConnectionUserID = string.Empty;
            string l_strConnectionPassword = string.Empty;

            if (!string.IsNullOrWhiteSpace(g_Config[uid]) && !string.IsNullOrWhiteSpace(g_Config[psw]))
            {
                l_strConnectionUserID = Function.IncEncrypt.GeneralDecrypt(g_Config[uid]);
                l_strConnectionPassword = Function.IncEncrypt.GeneralDecrypt(g_Config[psw]);
            }
            return string.Format(g_Config[connstr], l_strConnectionUserID, l_strConnectionPassword);
        }

    }
}
