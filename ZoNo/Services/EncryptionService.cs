using System.Security.Cryptography;
using System.Text;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class EncryptionService : IEncryptionService
  {
    private const string _entropy = "Nora is the love of my life";

    public string Encrypt(string stringToEncrypt)
    {
      return Convert.ToBase64String(
        ProtectedData.Protect(
          userData: Encoding.UTF8.GetBytes(stringToEncrypt),
          optionalEntropy: Encoding.UTF8.GetBytes(_entropy),
          scope: DataProtectionScope.CurrentUser));
    }

    public string Decrypt(string encryptedString)
    {
      return Encoding.UTF8.GetString(
        ProtectedData.Unprotect(
          encryptedData: Convert.FromBase64String(encryptedString),
          optionalEntropy: Encoding.UTF8.GetBytes(_entropy),
          scope: DataProtectionScope.CurrentUser));
    }
  }
}
