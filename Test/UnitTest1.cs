using Bitcoin_Address_Validation.Enums;
using Bitcoin_Address_Validation.Models;
using Bitcoin_Address_Validation.Services;
using System.Net;

namespace Test
{
    public class UnitTest1
    {

        private static IEnumerable<object[]> Addresses() => new List<object[]> {

            //Electrum
        {  new object[] {  AddressType.p2wpkh,  Network.testnet,  true, "tb1q9m4xfpn8efx2dtmufvydj32l5em3sgzd5s94nf" } },
        //?
        {  new object[] {  AddressType.p2pkh,  Network.testnet,  false, "mpn7M35mSjxerg5CD8qA1p4CjARvrpoigE" } },

        {  new object[] {  AddressType.p2pkh,  Network.mainnet,  false, "1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2" } },
        {  new object[] {  AddressType.p2sh,  Network.mainnet,  false, "3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy" } },
        {  new object[] {  AddressType.p2wpkh,  Network.mainnet,  true, "bc1qar0srrr7xfkvy5l643lydnw9re59gtzzwf5mdq" } },

        {  new object[] {  AddressType.p2pkh,  Network.mainnet,  false,  "17VZNX1SN5NtKa8UQFxwQbFeFc3iqRYhem" } },
        {  new object[]  {  AddressType.p2pkh, Network.testnet,  false,  "mipcBbFg9gMiCh81Kj8tqqdgoZub1ZJRfn" } },

        {  new object[]  {  AddressType.p2sh, Network.mainnet,  false,  "3J98t1WpEZ73CNmQviecrnyiWrnqRhWNLy" } },
        {  new object[]  {  AddressType.p2sh, Network.testnet,  false,  "2MzQwSSnBHWHqSAqtTVQ6v47XtaisrJa1Vc" } },


        {  new object[]  {  AddressType.p2wpkh, Network.mainnet,  true,  "bc1q973xrrgje6etkkn9q9azzsgpxeddats8ckvp5s" } },
        {  new object[]  {  AddressType.p2wpkh, Network.mainnet,  true,  "BC1Q973XRRGJE6ETKKN9Q9AZZSGPXEDDATS8CKVP5S" } },

        {  new object[]   {  AddressType.p2wpkh, Network.mainnet,  true, "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4" } },
        {  new object[]  {  AddressType.p2wpkh, Network.mainnet,  true,  "BC1QW508D6QEJXTDG4Y5R3ZARVARY0C5XW7KV8F3T4" } },

        {  new object[]  {  AddressType.p2wpkh, Network.testnet,  true,  "tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx" } },
        {  new object[]  {  AddressType.p2wpkh, Network.regtest,  true,  "bcrt1q6z64a43mjgkcq0ul2znwneq3spghrlau9slefp" } },

        {  new object[] {  AddressType.p2tr, Network.mainnet,  true,  "bc1ptxs597p3fnpd8gwut5p467ulsydae3rp9z75hd99w8k3ljr9g9rqx6ynaw" } },
        {  new object[] {  AddressType.p2tr, Network.testnet,  true,  "tb1p84x2ryuyfevgnlpnxt9f39gm7r68gwtvllxqe5w2n5ru00s9aquslzggwq" } },
        {  new object[] {  AddressType.p2tr, Network.regtest,  true,  "bcrt1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vqc8gma6" } },

        {  new object[] {  AddressType.p2wsh, Network.mainnet,  true,  "bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3" } },
        {  new object[] {  AddressType.p2wsh, Network.testnet,  true,  "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sl5k7" } },
        {  new object[] {  AddressType.p2wsh, Network.regtest,  true, "bcrt1q5n2k3frgpxces3dsw4qfpqk4kksv0cz96pldxdwxrrw0d5ud5hcqzzx7zt" } },

      //  fails
        //{  new object[] {  AddressType.p2tr,  Network.testnet,  false, "bc1pmzfrwwndsqmk5yh69yjr5lfgfg4ev8c0tsc06e" } },//Invalid checksum
        //{  new object[] {  AddressType.p2pkh, Network.testnet,  false,  "17VZNX1SN5NtKa8UFFxwQbFeFc3iqRYhem" } },//Invalid address
        //{  new object[] {  AddressType.p2pkh, Network.testnet,  false,  "bc1qw508d6qejxtdg4y5r3zrrvary0c5xw7kv8f3t4" } },//Invalid checksum

                                                                    };
        [Theory]
        [MemberData(nameof(Addresses))]
        public void TestAddressType(AddressType addressType, Network network, bool bech32, string address)
        {
            var addressInfo = Decoder.GetAddressInfo(address);

            Assert.Equal(addressType, addressInfo.Type);
        }

        [Theory]
        [MemberData(nameof(Addresses))]
        public void TestNetwork(AddressType addressType, Network network, bool bech32, string address)
        {
            var addressInfo = Decoder.GetAddressInfo(address);

            Assert.Equal(network, addressInfo.Network);
        }

        [Theory]
        [MemberData(nameof(Addresses))]
        public void TestIsBech(AddressType addressType, Network network, bool bech32, string address)
        {
            var addressInfo = Decoder.GetAddressInfo(address);

            Assert.Equal(bech32, addressInfo.Bech32);
        }

        [Theory]
        [MemberData(nameof(Addresses))]
        public void TestValidate(AddressType addressType, Network network, bool bech32, string address)
        {
            var result = Decoder.Validate(address, network);

            Assert.True(result);
        }
    }
}