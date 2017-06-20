using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
namespace KK.Frame.Loader.ABS
{
    public class HashUtil
    {
        public static string Get(Stream fs)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(fs);
            fs.Close();
            return ToHexString(bytes);
        }

        public static string Get(string s)
        {
             
            return Get(Encoding.UTF8.GetBytes(s));
        }
        public static string Md5Sum(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

           public static string Get(byte[] data)
             {
                  

                MD5 md5 = MD5.Create();
             
               
                 try
                 {
                     byte[] bytes = md5.ComputeHash(data);
                      
                     return ToHexString(bytes);
                 }
                 catch(Exception e)
                 {
                    // RoomResolveData.Instance.RecordMsg(e.Message);
                 }



                  


                 return "";
             }

             public static string ToHexString(byte[] bytes)
             {
                  
                 string hexString = string.Empty;
                 if (bytes != null)
                 {
                     StringBuilder strB = new StringBuilder();

                     for (int i = 0; i < bytes.Length; i++)
                     {
                         strB.Append(bytes[i].ToString("X2"));
                     }
                     hexString = strB.ToString().ToLower();
                 }
                  
                 return hexString;
             } 
    }
}
