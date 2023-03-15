using System.Collections.Generic;
using System.Linq;

namespace NetWorkLibrary.Algorithm
{
    public class RC4
    {
        private int x, y;
        byte[] state;

        public RC4(byte[] key)
        {
            x = 0;
            y = 0;
            EncrytInitizlize(key);
        }

        public byte[] Encrypt(byte[] data)
        {
            return EncryptOutput(data).ToArray(); ;
        }

        private void EncrytInitizlize(byte[] key)
        {
            state = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + key[i % key.Length] + state[i]) & 255;
                Swap(i, j);
            }
        }

        private IEnumerable<byte> EncryptOutput(IEnumerable<byte> data)
        {
            return data.Select((b) =>
            {
                x = (x + 1) & 255;
                y = (y + state[x]) & 255;
                Swap(x, y);
                return (byte)(b ^ state[(state[x] + state[y]) & 255]);
            });
        }

        private void Swap(int i, int j)
        {
            byte c = state[i];
            state[i] = state[j];
            state[j] = c;
        }
    }
}
