using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services
{
   
    public class AesEncryptionService
    {
        private static readonly byte[] Salted__ = Encoding.ASCII.GetBytes("Salted__");
        // Шифрует текст с помощью мастер-пароля
        public string Encrypt(string plainText, string password)
        {
            //Генерируем случайную соль(8 байт)
            byte[] salt = new byte[8]; 
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            // Получаем ключ и вектор инициализации (IV) через алгоритм, как в OpenSSL
            var keyAndIv = EvpKdf(Encoding.UTF8.GetBytes(password), salt, 32, 16); 
            byte[] key = keyAndIv.Take(32).ToArray();
            byte[] iv = keyAndIv.Skip(32).Take(16).ToArray();
            // Настраиваем шифрование AES
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
           
            using var ms = new MemoryStream();
            // Записываем заголовок и соль
            ms.Write(Salted__, 0, Salted__.Length);
            ms.Write(salt, 0, salt.Length);
            // Шифруем данные
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
            }
            // Возвращаем результат в Base64
            return Convert.ToBase64String(ms.ToArray());
        }
        // Расшифровывает текст, зашифрованный методом Encrypt
        public string Decrypt(string base64Cipher, string password)
        {
            byte[] cipher = Convert.FromBase64String(base64Cipher);
           
            if (cipher.Length < 16) throw new ArgumentException("Invalid cipher text");
            if (!cipher.Take(Salted__.Length).SequenceEqual(Salted__))
                throw new ArgumentException("Invalid salt");
            // Извлекаем соль и зашифрованные данные
            byte[] salt = cipher.Skip(Salted__.Length).Take(8).ToArray();
            byte[] encryptedData = cipher.Skip(Salted__.Length + 8).ToArray();
            // Генерируем тот же ключ и IV
            var keyAndIv = EvpKdf(Encoding.UTF8.GetBytes(password), salt, 32, 16);
            byte[] key = keyAndIv.Take(32).ToArray();
            byte[] iv = keyAndIv.Skip(32).Take(16).ToArray();
            // Расшифровываем
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        // Алгоритм генерации ключа (как в OpenSSL) — нужен для совместимости с JavaScript
        private static byte[] EvpKdf(byte[] password, byte[] salt, int keySize, int ivSize)
        {
            using var sha256 = SHA256.Create();
            int keyAndIvSize = keySize + ivSize;
            byte[] keyAndIv = new byte[keyAndIvSize];
            int offset = 0;
            var hash = new byte[0];
         
            while (offset < keyAndIvSize)
            {
                var hashInput = new byte[hash.Length + password.Length + salt.Length];
                Array.Copy(hash, hashInput, hash.Length);
                Array.Copy(password, 0, hashInput, hash.Length, password.Length);
                Array.Copy(salt, 0, hashInput, hash.Length + password.Length, salt.Length);
                hash = sha256.ComputeHash(hashInput);
                Array.Copy(hash, 0, keyAndIv, offset, Math.Min(hash.Length, keyAndIvSize - offset));
                offset += hash.Length;
            }

            return keyAndIv;
        }
    }
}
