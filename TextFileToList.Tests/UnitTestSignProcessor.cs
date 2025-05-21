using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace TextFileToList.Tests
{
    public class UnitTestSignProcessor
    {
        [Fact]
        public void Sign_ShouldCreateSignedFile_WhenValidInputsAreProvided()
        {
            // Arrange
            string testFileName = "testfile.txt";
            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);
            File.WriteAllText(testFilePath, "This is a test file.");

            using (var rsa = new RSACryptoServiceProvider())
            {
                string privateKeyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                // Act
                SignProcessor.Sign(testFilePath, privateKeyBase64);

                // Assert
                string signedFilePath = testFilePath + ".signed";
                Assert.True(File.Exists(signedFilePath), "Signed file was not created.");

                // Cleanup
                if (File.Exists(signedFilePath))
                {
                    File.Delete(signedFilePath);
                }
            }

            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Fact]
        public void Sign_ShouldThrowException_WhenFileDoesNotExist()
        {
            // Arrange
            string nonExistentFile = "nonexistent.txt";
            using (var rsa = new RSACryptoServiceProvider())
            {
                string privateKeyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                // Act & Assert
                Assert.Throws<FileNotFoundException>(() => SignProcessor.Sign(nonExistentFile, privateKeyBase64));
            }
        }

        [Fact]
        public void Sign_ShouldThrowException_WhenPrivateKeyIsInvalid()
        {
            // Arrange
            string testFileName = "testfile.txt";
            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);
            File.WriteAllText(testFilePath, "This is a test file.");

            string invalidPrivateKey = "InvalidKey";

            // Act & Assert
            Assert.Throws<FormatException>(() => SignProcessor.Sign(testFilePath, invalidPrivateKey));

            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Fact]
        public void Verify_ShouldReturnTrue_WhenSignatureIsValid()
        {
            // Arrange
            string testFileName = "testfile.txt";
            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);
            File.WriteAllText(testFilePath, "This is a test file.");

            using (var rsa = new RSACryptoServiceProvider())
            {
                string privateKeyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                string publicKeyBase64 = Convert.ToBase64String(rsa.ExportRSAPublicKey());

                SignProcessor.Sign(testFilePath, privateKeyBase64);

                string signedFilePath = testFilePath + ".signed";

                // Act
                bool isValid = SignProcessor.Verify(signedFilePath, publicKeyBase64);

                // Assert
                Assert.True(isValid, "The signature should be valid.");

                // Cleanup
                if (File.Exists(signedFilePath))
                {
                    File.Delete(signedFilePath);
                }
            }

            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenSignatureIsInvalid()
        {
            // Arrange
            string testFileName = "testfile.txt";
            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);
            File.WriteAllText(testFilePath, "This is a test file.");

            using (var rsa = new RSACryptoServiceProvider())
            {
                string privateKeyBase64 = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                SignProcessor.Sign(testFilePath, privateKeyBase64);

                string signedFilePath = testFilePath + ".signed";

                using (var anotherRsa = new RSACryptoServiceProvider())
                {
                    string differentPublicKeyBase64 = Convert.ToBase64String(anotherRsa.ExportRSAPublicKey());

                    // Act
                    bool isValid = SignProcessor.Verify(signedFilePath, differentPublicKeyBase64);

                    // Assert
                    Assert.False(isValid, "The signature should be invalid.");
                }

                // Cleanup
                if (File.Exists(signedFilePath))
                {
                    File.Delete(signedFilePath);
                }
            }

            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }
}
