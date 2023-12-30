using System.Security.Cryptography;

namespace ZoNo.Contracts.Services
{
  public interface IEncryptionService
  {
    /// <summary>
    /// Encrypts a string by using the <see cref="ProtectedData"/> class.
    /// </summary>
    /// <param name="stringToEncrypt">String to encrypt.</param>
    /// <returns>Encrypted string.</returns>
    string Encrypt(string stringToEncrypt);

    /// <summary>
    /// Decrypts a string by using the <see cref="ProtectedData"/> class.
    /// </summary>
    /// <param name="encryptedString">String to decrypt.</param>
    /// <returns>Decrypted string.</returns>
    string Decrypt(string encryptedString);
  }
}
