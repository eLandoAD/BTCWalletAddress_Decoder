namespace Bitcoin_Address_Validation.Library
{
    public class Lib
    {
        string ALPHABET = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        Dictionary<Char, byte> ALPHABET_MAP = new();
        public Lib()
        {
            for (byte z = 0; z < ALPHABET.Length; z++)
            {
                var x = ALPHABET[z];
                ALPHABET_MAP.Add(x, z);
            }
        }
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
                throw new Exception($"Wallet {address} is too short");
            if (address.Length > limit)
                throw new Exception($"Wallet {address} is too short");

            // don't allow mixed case
            var lowered = address.ToLower();
            var uppered = address.ToUpper();

            if (address.Equals(lowered) && address.Equals(uppered))
            {
                throw new Exception($"Mixed-case string  {address}");
            }

            address = lowered;

            var split = address.LastIndexOf('1');
            if (split == -1)
            {
                throw new Exception($"No separator character for {address}");
            }
            if (split == 0)
            {
                throw new Exception($"Missing prefix for {address}");
            }
            prefix = string.Join("", address.Take(split));

            var wordChars = string.Join("", address.Skip(split + 1));

            if (wordChars.Length < 6)
            {
                throw new Exception($"Data {wordChars} is too short");
            }

            int chk = PrefixChk(prefix);

            List<byte> _data = new();
            for (var i = 0; i < wordChars.Length; ++i)
            {
                var c = wordChars[i];

                if (!ALPHABET_MAP.TryGetValue(c, out byte v))
                {
                    throw new Exception($"Unknown character {c}");
                }
                chk = polymodStep(chk) ^ v;

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

        public int PrefixChk(string prefix)
        {
            var chk = 1;
            for (var i = 0; i < prefix.Length; ++i)
            {
                var c = prefix.ElementAt(i);
                if (c < 33 || c > 126)
                {
                    throw new Exception($"Invalid prefix ( {prefix})");
                }
                chk = polymodStep(chk) ^ (c >> 5);
            }
            chk = polymodStep(chk);
            for (var i = 0; i < prefix.Length; ++i)
            {
                var v = prefix.ElementAt(i);
                chk = polymodStep(chk) ^ (v & 0x1f);
            }
            return chk;
        }
        private int polymodStep(int pre)
        {
            var b = pre >> 25;
            int result = (((pre & 0x1ffffff) << 5) ^
                (-((b >> 0) & 1) & 0x3b6a57b2) ^
                (-((b >> 1) & 1) & 0x26508e6d) ^
                (-((b >> 2) & 1) & 0x1ea119fa) ^
                (-((b >> 3) & 1) & 0x3d4233dd) ^
                (-((b >> 4) & 1) & 0x2a1462b3));
            return result;
        }


        private byte[]? convert(byte[]? data, byte inBits, byte outBits, bool pad)
        {
            int value = 0;
            int bits = 0;
            int maxV = (1 << outBits) - 1;
            List<byte> result = new(); ;
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
                    throw new Exception("Excess padding");
                }
                int? actual = (value << (outBits - bits)) & maxV;
                if (actual is not null && actual!=0)
                {
                    throw new Exception("Non-zero padding");
                }
            }
            return result.ToArray();
        }
        //function toWords(bytes)
        //{
        //    return convert(bytes, 8, 5, true);
        //}
        //function fromWordsUnsafe(words)
        //{
        //    const res = convert(words, 5, 8, false);
        //    if (Array.isArray(res))
        //        return res;
        //}
        public byte[]? FromWords(byte[] words)
        {
            var res = convert(words, 5, 8, false);
            return res;
        }

    }
}
