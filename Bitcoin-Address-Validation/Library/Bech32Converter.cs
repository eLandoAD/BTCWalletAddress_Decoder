namespace Bitcoin_Address_Validation.Library
{
    public class Bech32Converter
    {
        #region Props
        private readonly string ALPHABET = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        private readonly Dictionary<Char, byte> ALPHABET_MAP = new();
        #endregion

        #region Ctor
        public Bech32Converter()
        {
            for (byte z = 0; z < ALPHABET.Length; z++)
            {
                var x = ALPHABET[z];
                ALPHABET_MAP.Add(x, z);
            }
        }
        #endregion

        public void Decode(string address, string encoding, int? limit, out string? prefix, out byte[]? data)
        {
            int ENCODING_CONST;
            if (!encoding.Equals("m"))
            {
                ENCODING_CONST = 1;
            }
            else
            {
                ENCODING_CONST = 0x2bc830a3;
            }
            limit ??= 90;
            if (address.Length < 8)
            {
                throw new Exception($"Wallet {address} is too short!");
            }
            if (address.Length > limit)
            {
                throw new Exception($"Wallet {address} is too long!");
            }
            // does not allow shaffle cases
            var lowered = address.ToLower();

            if (address.Equals(lowered) && address.Equals(address.ToUpper()))
            {
                throw new Exception($"Shuffled case chars in wallet: {address}");
            }

            address = lowered;

            var split = address.LastIndexOf('1');

            if (split == -1)
            {
                throw new Exception($"Missing separator character for wallet: {address}");
            }
            if (split == 0)
            {
                throw new Exception($"Missing prefix for wallet: {address}");
            }
            prefix = string.Join("", address.Take(split));

            var wordChars = string.Join("", address.Skip(split + 1));

            if (wordChars.Length < 6)
            {
                throw new Exception($"Too short DATA: {wordChars} for wallet: {address}");
            }

            int chk = PrefixChk(prefix);

            List<byte> _data = new();
            for (var i = 0; i < wordChars.Length; ++i)
            {
                var c = wordChars[i];

                if (!ALPHABET_MAP.TryGetValue(c, out byte v))
                {
                    throw new Exception($"Unknown char {c}");
                }
                chk = PolyModeStep(chk) ^ v;

                // not in the checksum?
                if (i + 6 >= wordChars.Length)
                {
                    continue;
                }
                _data.Add(v);
            }
            if (chk != ENCODING_CONST)
            {
                throw new Exception($"Invalid checksum for {address}");
            }
            data = _data.ToArray();
        }

        public static byte[]? Convert5to8(byte[] bytes)
        {
            var result = Convert(bytes, 5, 8, false);
            return result;
        }

        public static byte[]? Convert8to5(byte[] bytes)
        {
            var res = Convert(bytes, 8, 5, true);
            return res;
        }

        private static int PrefixChk(string prefix)
        {
            var chk = 1;
            for (var i = 0; i < prefix.Length; ++i)
            {
                var c = prefix.ElementAt(i);
                if (c <= 32 || c >= 127)
                {
                    throw new Exception($"Not valid prefix ({prefix})");
                }
                chk = PolyModeStep(chk) ^ (c >> 5);
            }
            chk = PolyModeStep(chk);
            for (var i = 0; i < prefix.Length; ++i)
            {
                var v = prefix.ElementAt(i);
                chk = PolyModeStep(chk) ^ (v & 0x1f);
            }
            return chk;
        }

        private static int PolyModeStep(int value)
        {
            var b = value >> 25;
            int result = (((value & 0x1ffffff) << 5) ^
                (-((b >> 0) & 1) & 0x3b6a57b2) ^
                (-((b >> 1) & 1) & 0x26508e6d) ^
                (-((b >> 2) & 1) & 0x1ea119fa) ^
                (-((b >> 3) & 1) & 0x3d4233dd) ^
                (-((b >> 4) & 1) & 0x2a1462b3));

            return result;
        }

        private static byte[]? Convert(byte[]? data, byte inBits, byte outBits, bool pad)
        {
            int value = 0;
            int bits = 0;
            int maxV = (1 << outBits) - 1;
            List<byte> result = new();
            if (data is null)
            {
                throw new Exception("Coneverter: received null param");
            }
            for (var i = 0; i < data.Length; ++i)
            {
                value = (value << inBits) | data[i];
                bits += inBits;
                while (bits >= outBits)
                {
                    bits -= outBits;
                    result.Add((byte)((value >> bits) & maxV));
                }
            }
            if (pad)
            {
                if (bits > 0)
                {
                    result.Add((byte)((value << (outBits - bits)) & maxV));
                }
            }
            else
            {
                if (bits >= inBits)
                {
                    throw new Exception("Coneverter: Excess padding");
                }
                int? actual = (value << (outBits - bits)) & maxV;
                if (actual is not null && actual != 0)
                {
                    throw new Exception("Coneverter: Zero(0) padding ");
                }
            }
            return result.ToArray();
        }
    }
}
