using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TextFileToList
{
    public static class SignProcessor
    {
        public static void Sign(string filePath, string privateKeyBase64)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(privateKeyBase64))
            {
                throw new ArgumentException("Private key cannot be null or empty.", nameof(privateKeyBase64));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            // Decode the private key from Base64
            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

            // Read the file content
            byte[] fileContent = File.ReadAllBytes(filePath);

            // Compute the hash using SHA512
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hash = sha512.ComputeHash(fileContent);

                // Sign the hash with the private key
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                    byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

                    // Save the signature to a new file
                    string signedFilePath = filePath + ".signed";
                    File.WriteAllBytes(signedFilePath, signature);
                }
            }
        }

        public static bool Verify(string signedFilePath, string publicKeyBase64)
        {
            if (string.IsNullOrWhiteSpace(signedFilePath))
            {
                throw new ArgumentException("Signed file path cannot be null or empty.", nameof(signedFilePath));
            }

            if (string.IsNullOrWhiteSpace(publicKeyBase64))
            {
                throw new ArgumentException("Public key cannot be null or empty.", nameof(publicKeyBase64));
            }

            if (!File.Exists(signedFilePath))
            {
                throw new FileNotFoundException("The specified signed file does not exist.", signedFilePath);
            }

            // Decode the public key from Base64
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);

            // Read the signed file content
            byte[] signature = File.ReadAllBytes(signedFilePath);

            // Extract the original file path
            string originalFilePath = signedFilePath.Replace(".signed", "");
            if (!File.Exists(originalFilePath))
            {
                throw new FileNotFoundException("The original file corresponding to the signed file does not exist.", originalFilePath);
            }

            byte[] originalFileContent = File.ReadAllBytes(originalFilePath);

            // Compute the hash of the original file
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hash = sha512.ComputeHash(originalFileContent);

                // Verify the signature with the public key
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                    return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                }
            }
        }

        public static RSACryptoServiceProvider BuildRSACryptoServiceProvider(string privateKeyBase64, string publicKeyBase64)
        {
            if (string.IsNullOrWhiteSpace(privateKeyBase64) && string.IsNullOrWhiteSpace(publicKeyBase64))
            {
                throw new ArgumentException("At least one of the keys (private or public) must be provided.");
            }

            var rsa = new RSACryptoServiceProvider();

            if (!string.IsNullOrWhiteSpace(privateKeyBase64))
            {
                byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }

            if (!string.IsNullOrWhiteSpace(publicKeyBase64))
            {
                byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);
            }

            return rsa;
        }
    }
}
