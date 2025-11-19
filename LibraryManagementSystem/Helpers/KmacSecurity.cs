using System.Text;

namespace LibraryManagementSystem.Helpers;

public static class KmacSecurity
{
    public static byte[] DeriveKmacKey(string userId, IEnumerable<string> roles, string email, byte[] secret)
    {
        var rolesString = string.Join(",", roles.OrderBy(r => r));
        var inputData = Encoding.UTF8.GetBytes($"{userId}|{rolesString}|{email}");

        var kmac = new Org.BouncyCastle.Crypto.Macs.KMac(256, secret);
        kmac.Init(new Org.BouncyCastle.Crypto.Parameters.KeyParameter(secret));
        kmac.BlockUpdate(inputData, 0, inputData.Length);
        var output = new byte[kmac.GetMacSize()];
        kmac.DoFinal(output, 0);
        return output;
    }
}
