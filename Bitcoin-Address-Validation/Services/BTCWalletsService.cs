namespace Bitcoin_Address_Validation.Services
{
    #region Usings
    using System.Security.Cryptography;

    using Bitcoin_Address_Validation.Enums;
    using Bitcoin_Address_Validation.Library;
    using Bitcoin_Address_Validation.Models;
    #endregion

    public class BTCWalletsService : IBTCWalletsService
    {
        #region Props
        private readonly Dictionary<string, Network> MapPrefixToNetwork = new() { { "bc", Network.MAINNET }, { "tb", Network.TESTNET }, { "bcrt", Network.REGTEST } };
        private readonly Dictionary<int, AddressInfo> AddressTypes = new(){
                                                                    {0x00, new AddressInfo {Type= AddressType.P2PKH, Network= Network.MAINNET } },
                                                                    {0x6f, new AddressInfo {Type= AddressType.P2PKH, Network= Network.TESTNET} },
                                                                    {0x05, new AddressInfo {Type= AddressType.P2SH, Network= Network.MAINNET } },
                                                                    {0xc4, new AddressInfo {Type= AddressType.P2SH, Network= Network.TESTNET } }
                                                                };
        #endregion

        public bool Validate(string address, Network? network)
        {
            try
            {
                var addressInfo = GetAddressInfo(address);
                return network is null || network.Equals(addressInfo.Network);
            }
            catch
            {
                return false;
            }
        }

        public AddressInfo GetAddressInfo(string address)
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

                var length = decoded.Length;

                if (length != 25)
                {
                    throw new Exception("Invalid address (decoded length)");
                }

                var version = decoded[0];

                var checksum = decoded.Skip(length - 4).ToArray();
                var body = decoded.Take(length - 4).ToArray();
                var expectedChecksum = SHA256.HashData(SHA256.HashData(body)).Take(4).ToArray();

                var exist = checksum.Where((value, index) => value != expectedChecksum[index]).ToArray();

                if (exist.Any())
                {
                    throw new Exception("Invalid address (check sums)");
                }

                var versionHex = Convert.ToInt32(version);

                bool validVersions = AddressTypes.ContainsKey(versionHex);

                if (!validVersions)
                {
                    throw new Exception("Invalid address (version)");
                }

                var addressType = this.AddressTypes[version];

                addressType.Address = address;
                addressType.Bech32 = false;

                return addressType;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        private AddressInfo ParseBech32(string address)
        {
            var addressInfo = new AddressInfo { Bech32 = true };

            try
            {
                var bech32Converter = new Bech32Converter();
                string bech = "";
                if (address.StartsWith("bc1p") || address.StartsWith("tb1p") || address.StartsWith("bcrt1p"))
                {
                    bech = "m";
                }

                bech32Converter.Decode(address, bech, null, out string? decodeString, out byte[]? decodeByteArr);

                if (string.IsNullOrEmpty(decodeString) || decodeByteArr is null || decodeByteArr.Length == 0)
                {
                    throw new Exception("Invalid address (prefix/bytes)");
                }


                var hasNetwork = this.MapPrefixToNetwork.TryGetValue(decodeString, out Network network);
                if (!hasNetwork)
                {
                    throw new Exception("Invalid address (network)");
                }

                addressInfo.Network = network;

                var witnessVersion = Convert.ToInt32(decodeByteArr[0]);

                if (witnessVersion < 0 || witnessVersion > 16)
                {
                    throw new Exception("Invalid address (version)");
                }

                byte[] bytes = decodeByteArr.Skip(1).ToArray();
                byte[]? data = Bech32Converter.Convert5to8(bytes);


                if (data is not null && data.Length == 20)
                {
                    addressInfo.Type = AddressType.P2PWPKH;
                }
                else if (witnessVersion == 1)
                {
                    addressInfo.Type = AddressType.P2TR;
                }
                else
                {
                    addressInfo.Type = AddressType.P2WSH;
                }

                return addressInfo;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
