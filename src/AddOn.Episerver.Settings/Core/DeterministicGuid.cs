using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AddOn.Episerver.Settings.Core;

internal static class DeterministicGuid
{
    /// <summary>
    ///     Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    ///     Slimmed down version of:
    ///     https://github.com/Faithlife/FaithlifeUtility/blob/master/src/Faithlife.Utility/GuidUtility.cs
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="name">The name (within that namespace).</param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    internal static Guid Create(Guid namespaceId, string name)
    {
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);
        var data = namespaceBytes.Concat(nameBytes).ToArray();
        byte[] hash;
        using (var algorithm = SHA1.Create())
        {
            hash = algorithm.ComputeHash(data);
        }

        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);
        newGuid[6] = (byte)(newGuid[6] & 0x0F | 5 << 4);
        newGuid[8] = (byte)(newGuid[8] & 0x3F | 0x80);
        SwapByteOrder(newGuid);

        return new Guid(newGuid);
    }

    private static void SwapByteOrder(byte[] guid)
    {
        SwapBytes(guid, 0, 3);
        SwapBytes(guid, 1, 2);
        SwapBytes(guid, 4, 5);
        SwapBytes(guid, 6, 7);
    }

    private static void SwapBytes(byte[] guid, int left, int right)
    {
        (guid[left], guid[right]) = (guid[right], guid[left]);
    }
}
