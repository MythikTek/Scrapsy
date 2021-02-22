using System.IO;
using System.Security.Cryptography;

namespace scrapsy.Stores.Data
{
    public class BestBuyAuthentication
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] PasswordKey { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Cvv { get; set; }
        public string CvvKey { get; set; }
        public string CvvSalt { get; set; }

        public static void Serialize(string path, BestBuyAuthentication authentication)
        {
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(authentication.Email);
                writer.Write(authentication.PasswordKey);
                writer.Write(authentication.PasswordSalt);
                writer.Write(authentication.CvvKey);
                writer.Write(authentication.CvvSalt);
            }
        }

        public static void Encrypt(ref BestBuyAuthentication authentication)
        {
            using (var derivedBytes = new Rfc2898DeriveBytes(authentication.Password, 20))
            {
                authentication.PasswordSalt = derivedBytes.Salt;
                authentication.PasswordKey = derivedBytes.GetBytes(20);
            }
        }

        public static void Decrypt(ref BestBuyAuthentication authentication, string password)
        {
            using (var derivedBytes = new Rfc2898DeriveBytes(password, authentication.PasswordSalt))
            {
                var newKey = derivedBytes.GetBytes(20);
            }
        }

        public static BestBuyAuthentication DeSerialize(string path)
        {
            BestBuyAuthentication auth;
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                auth = new BestBuyAuthentication
                {
                    Email = reader.ReadString(),
                    PasswordKey = reader.ReadBytes(20),
                    PasswordSalt = reader.ReadBytes(20),
                    CvvKey = reader.ReadString(),
                    CvvSalt = reader.ReadString()
                };
            }

            return auth;
        }
    }
}