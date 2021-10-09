using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkLibrary.Algorithm
{
    public class MPPD
    {
        private const int BLOCK_SIZE = 0x2000;
        private const int CTRL_OFF_EOB = 0;

        private readonly byte[] history;
        private int histHead;
        private int histoff;
        private List<byte> legacy;
        private int l;
        private int rIndex;
        private long totallen, blen;
        private int lastl, lastrIndex;

        public MPPD()
        {
            history = new byte[BLOCK_SIZE];
            histoff = 0;
            histHead = 0;
            legacy = new List<byte>();
            l = 0;
        }

        public byte[] DeCompress(byte[] input)
        {
            byte[] ret;
            if (input.Length > 0 || legacy.Count > 0)
            {
                ret = Update(input);
            }
            else
            {
                ret = input;
            }
            return ret;
        }

        private byte[] Update(byte[] input)
        {
            legacy.AddRange(input);
            totallen = legacy.Count * 8 - l;
            rIndex = 0;
            blen = 7;
            histHead = histoff;
            MemoryStream retMs = new MemoryStream();
            while (totallen > blen)
            {
                lastl = l;
                lastrIndex = rIndex;
                var val = Fetch();
                if (val < 0x80000000)
                {
                    if (!Passbit(8)) break;
                    history[histoff++] = (byte)(val >> 24);
                    continue;
                }
                if (val < 0xc0000000)
                {
                    if (!Passbit(9)) break;
                    history[histoff++] = (byte)(((val >> 23) | 0x80) & 0xff);
                    continue;
                }

                uint off = 0, len;
                if (val >= 0xf0000000)
                {
                    if (!Passbit(10)) break;
                    off = (val >> 22) & 0x3f;
                    if (off == CTRL_OFF_EOB)
                    {
                        var advance = 8 - (l & 7);
                        if (advance < 8)
                            if (!Passbit(advance)) break;
                        retMs.Write(history, histHead, histoff - histHead);
                        if (histoff == BLOCK_SIZE)
                            histoff = 0;
                        histHead = histoff;
                        continue;
                    }
                }
                else if (val >= 0xe0000000)
                {
                    if (!Passbit(12)) break;
                    off = ((val >> 20) & 0xff) + 64;
                }
                else if (val >= 0xe0000000)
                {
                    if (!Passbit(16)) break;
                    off = ((val >> 16) & 0x1fff) + 320;
                }

                val = Fetch();
                if (val < 0x80000000)
                {
                    if (!Passbit(1)) break;
                    len = 3;
                }
                else if (val < 0xc0000000)
                {
                    if (!Passbit(4)) break;
                    len = 4 | ((val >> 28) & 3);
                }
                else if (val < 0xe0000000)
                {
                    if (!Passbit(6)) break;
                    len = 8 | ((val >> 26) & 7);
                }
                else if (val < 0xf0000000)
                {
                    if (!Passbit(8)) break;
                    len = 16 | ((val >> 24) & 15);
                }
                else if (val < 0xf8000000)
                {
                    if (!Passbit(10)) break;
                    len = 32 | ((val >> 22) & 0x1f);
                }
                else if (val < 0xfc000000)
                {
                    if (!Passbit(12)) break;
                    len = 64 | ((val >> 20) & 0x3f);
                }
                else if (val < 0xfe000000)
                {
                    if (!Passbit(14)) break;
                    len = 128 | ((val >> 18) & 0x7f);
                }
                else if (val < 0xff000000)
                {
                    if (!Passbit(16)) break;
                    len = 256 | ((val >> 16) & 0xff);
                }
                else if (val < 0xff800000)
                {
                    if (!Passbit(18)) break;
                    len = 0x200 | ((val >> 14) & 0x1ff);
                }
                else if (val < 0xffc00000)
                {
                    if (!Passbit(20)) break;
                    len = 0x400 | ((val >> 12) & 0x3ff);
                }
                else if (val < 0xffe00000)
                {
                    if (!Passbit(22)) break;
                    len = 0x800 | ((val >> 10) & 0x7ff);
                }
                else if (val < 0xfff00000)
                {
                    if (!Passbit(24)) break;
                    len = 0x1000 | ((val >> 8) & 0xfff);
                }
                else
                {
                    l = lastl;
                    rIndex = lastrIndex;
                    break;
                }
                if (histoff - off < 0 || histoff + len > BLOCK_SIZE)
                    break;
                sameCopy((int)(histoff - off), (int)len);
            }
            retMs.Write(history, histHead, histoff - histHead);
            legacy.RemoveRange(0, rIndex);
            return retMs.ToArray();
        }

        private void sameCopy(int start, int len)
        {
            for (int i = 0; i < len; i++)
                history[histoff++] = history[start + i];
        }

        private uint Fetch()
        {
            rIndex += l >> 3;
            l &= 7;

            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (rIndex + i < legacy.Count)
                    data[i] = legacy[rIndex + i];
                else
                    data[i] = 0;
            }
            return (uint)(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data)) << l);
        }

        private bool Passbit(int len)
        {
            l += len;
            blen += len;
            if (blen < totallen)
                return true;

            rIndex = lastrIndex;
            l = lastl;
            return false;
        }
    }
}
