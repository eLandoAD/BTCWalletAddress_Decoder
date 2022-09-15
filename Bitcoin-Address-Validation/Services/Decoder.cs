namespace Bitcoin_Address_Validation.Services
{
    using Bitcoin_Address_Validation.Enums;
    using Bitcoin_Address_Validation.Library;
    using Bitcoin_Address_Validation.Models;
    using Nano.Bech32;
    using System.Security.Cryptography;
    using System.Text;

    public class Decoder
    {

        private static AddressInfo ParseBech32(string address)
        {
            //BechEncoder.Bech32Encoder.Decode(address, out string? decodeString, out byte[]? decodeByteArr);
            Lib lib = new Lib();
            string bech = "";
            if (address.StartsWith("bc1p") || address.StartsWith("tb1p") || address.StartsWith("bcrt1p"))
            {
                bech = "m";
            }
            lib.Decode(address, bech, null, out string? decodeString, out byte[]? decodeByteArr);

            if (string.IsNullOrEmpty(decodeString) || decodeByteArr is null || decodeByteArr.Length == 0)
            {
                throw new Exception("Invalid address");
            }

            var mapPrefixToNetwork = new Dictionary<string, Network>{
                                                                        { "bc", Network.MAINNET },
                                                                        { "tb", Network.TESTNET },
                                                                        { "bcrt", Network.REGTEST }
                                                                      };
            Network network;
            try
            {
                mapPrefixToNetwork.TryGetValue(decodeString, out network);


                var witnessVersion = Convert.ToInt32(decodeByteArr[0]);

                if (witnessVersion < 0 || witnessVersion > 16)
                {
                    throw new Exception("Invalid address");
                }
                byte[] bytes = decodeByteArr.Skip(1).ToArray();
                byte[]? data = lib.FromWords(bytes);

                AddressType type;

                if (data.Count() == 20)
                {
                    type = AddressType.P2PWPKH;
                }
                else if (witnessVersion == 1)
                {
                    type = AddressType.P2TR;
                }
                else
                {
                    type = AddressType.P2WSH;
                }

                return new AddressInfo
                {
                    Bech32 = true,
                    Network = network,
                    Address = address,
                    Type = type
                };
            }
            catch
            {
                throw new Exception("Invalid address");
            }
        }

        public static AddressInfo GetAddressInfo(string address)
        {
            byte[] decoded;
            string prefix = address.Substring(0, 2).ToLower();

            if (prefix.Equals("bc") || prefix.Equals("tb"))
            {
                return ParseBech32(address);
            }

            try
            {
                decoded = Base58.Decode(address);
            }
            catch (Exception)
            {
                throw new Exception("Invalid address");
            }

            var length = decoded.Length;

            if (length != 25)
            {
                throw new Exception("Invalid address");
            }

            var version = decoded[0];

            var checksum = decoded.Skip(length - 4).ToArray();
            var body = decoded.Take(length - 4).ToArray();
            var expectedChecksum = SHA256.HashData(SHA256.HashData(body)).Take(4).ToArray();

            var exist = checksum.Where((value, index) => value != expectedChecksum[index]).ToArray();

            if (exist.Any())
            {
                throw new Exception("Invalid address");
            }

            var versionHex = Convert.ToInt32(version);

            Dictionary<int, AddressInfo> AddressTypes = new Dictionary<int, AddressInfo> {
                                                                    {0x00, new AddressInfo {Type= AddressType.P2PKH, Network= Network.MAINNET } },
                                                                    {0x6f, new AddressInfo {Type= AddressType.P2PKH, Network= Network.TESTNET} },
                                                                    {0x05, new AddressInfo {Type= AddressType.P2SH, Network= Network.MAINNET } },
                                                                    {0xc4, new AddressInfo {Type= AddressType.P2SH, Network= Network.TESTNET } }
                                                                };
            bool validVersions = AddressTypes.ContainsKey(versionHex);

            if (!validVersions)
            {
                throw new Exception("Invalid address");
            }

            var addressType = AddressTypes[version];

            addressType.Address = address;
            addressType.Bech32 = false;

            return addressType;
        }

        public static bool Validate(string address, Network? network)
        {
            try
            {
                var addressInfo = GetAddressInfo(address);

                if (network is not null)
                {
                    return network.Equals(addressInfo.Network);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
