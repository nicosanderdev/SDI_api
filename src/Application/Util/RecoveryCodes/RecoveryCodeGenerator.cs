using System.Security.Cryptography;
using System.Text;
using SDI_Api.Application.Common.Interfaces;

namespace SDI_Api.Application.Util.RecoveryCodes;

public class RecoveryCodeGenerator : IRecoveryCodeGenerator
{
    private const string RecoveryCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public string GenerateCode()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 10; i++)
        {
            int index = RandomNumberGenerator.GetInt32(0, RecoveryCodeChars.Length);
            sb.Append(RecoveryCodeChars[index]);
        }
        return sb.ToString();
    }
}
