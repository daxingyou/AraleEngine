using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Arale.Engine
{

    public class Cipher
    {
    #if UNITY_IPHONE
        private const string PLATFORM_DLL = "__Internal";
    #elif UNITY_ANDROID
        private const string PLATFORM_DLL = "CryptAndroid";
    #else
        private const string PLATFORM_DLL = "Crypt";
    #endif

    #if !UNITY_IPHONE && !UNITY_EDITOR
        [DllImport(PLATFORM_DLL)]
        public static extern void Encrypt(ref byte data, int len, bool isFile);

        [DllImport(PLATFORM_DLL)]
        public static extern void Decrypt(ref byte data, int len, bool isFile);

        [DllImport(PLATFORM_DLL)]
        public static extern void BatchFileEncrypt(string filePath, bool IsEncrypt = true);

        [DllImport(PLATFORM_DLL)]
        public static extern uint _MaskNum_int(int value);

        [DllImport(PLATFORM_DLL)]
        public static extern uint _MaskNum_float(float value);

    #endif

        public static uint MaskNum(int value)
        {
    #if UNITY_EDITOR || UNITY_IPHONE
            return (uint)value ^ 0xf0e0d;
    #else
            return _MaskNum_int(value);
    #endif
        }

        public static uint MaskNum(float value)
        {
    #if UNITY_EDITOR || UNITY_IPHONE
            uint v = ((uint)Mathf.FloorToInt(value) ^ 0x10f0) |
                (0x234u ^ (uint)(1000000f * (value - Mathf.Floor(value))));
            return v;
    #else
            return _MaskNum_float(value);
    #endif
        }

        public static void NativeEncrypt(byte[] data, int len, bool isFile)
        {
    #if UNITY_EDITOR || UNITY_IPHONE
            for (int i = 0; i < len; ++i)
            {
                int offset = (i % 11) * 3;
                if (isFile)
                {
                    exchangeFile(ref data[i]);
                }
                else
                {
                    exchangeData(ref data[i]);
                }

                XOR(ref data[i], i);
                ROR(ref data[i], (int)((MAGIC_NUM) >> offset) & 0X7);
            }
    #else
                Encrypt(ref data[0], len, isFile);
    #endif
        }

        public static void NativeDecrypt(byte[] data, int len, bool isFile)
        {
    #if UNITY_EDITOR || UNITY_IPHONE
            for (int i = 0; i < len; ++i)
            {
                int offset = (i % 11) * 3;

                ROL(ref data[i], (int)((MAGIC_NUM) >> offset) & 0X7);
                XOR(ref data[i], i);
                if (isFile)
                {
                    exchangeFile(ref data[i]);
                }
                else
                {
                    exchangeData(ref data[i]);
                }
            }
    #else
                Decrypt(ref data[0], len, isFile);
    #endif
        }

    #if UNITY_EDITOR || UNITY_IPHONE
        const int BYTE = 8;
        const uint MAGIC_NUM = 0Xdeaddace;

        static void XOR(ref byte data, int i)
        {
            int offset = (i % 4) << 3;
            data ^= (byte)((MAGIC_NUM >> offset) & 0Xff);
        }

        static void ROL(ref byte data, int offset)
        {
            byte lastData = 0;
            byte[] total = new byte[] { data };
            BitArray new_array = new BitArray(8);
            BitArray old_array = new BitArray(total);

            for (int i = 0; i < BYTE; ++i)
            {
                int new_bit = (i + offset) % BYTE;
                new_array.Set(new_bit, old_array.Get(i));
            }

            for (int i = 0; i < BYTE; ++i)
            {
                bool value = new_array.Get(i);
                old_array.Set(i, value);
                if (value)
                {
                    lastData |= (byte)(1 << i);
                }
            }

            data = lastData;
        }

        static void ROR(ref byte data, int offset)
        {
            byte lastData = 0;
            byte[] total = new byte[] { data };
            BitArray new_array = new BitArray(8);
            BitArray old_array = new BitArray(total);

            for (int i = 0; i < BYTE; ++i)
            {
                int new_bit = (i - offset) % BYTE;
                if (new_bit < 0)
                {
                    new_bit += BYTE;
                }
                new_array.Set(new_bit, old_array.Get(i));
            }

            for (int i = 0; i < BYTE; ++i)
            {
                bool value = new_array.Get(i);
                old_array.Set(i, value);
                if (value)
                {
                    lastData |= (byte)(1 << i);
                }
            }

            data = lastData;
        }

        static void exchangeData(ref byte data)
        {
            byte[] total = new byte[] { data };
            BitArray old_array = new BitArray(total);
            BitArray new_array = new BitArray(old_array);

            new_array.Set(0, old_array.Get(4));
            new_array.Set(4, old_array.Get(0));

            new_array.Set(1, old_array.Get(3));
            new_array.Set(3, old_array.Get(1));

            new_array.Set(2, old_array.Get(5));
            new_array.Set(5, old_array.Get(2));

            new_array.Set(6, old_array.Get(7));
            new_array.Set(7, old_array.Get(6));

            byte lastData = 0;
            for (int i = 0; i < BYTE; ++i)
            {
                if (new_array.Get(i))
                {
                    lastData |= (byte)(1 << i);
                }
            }

            data = lastData;
        }

        static void exchangeFile(ref byte data)
        {
            byte[] total = new byte[] { data };
            BitArray old_array = new BitArray(total);
            BitArray new_array = new BitArray(old_array);

            new_array.Set(0, old_array.Get(6));
            new_array.Set(6, old_array.Get(0));

            new_array.Set(1, old_array.Get(5));
            new_array.Set(5, old_array.Get(1));

            new_array.Set(2, old_array.Get(3));
            new_array.Set(3, old_array.Get(2));

            new_array.Set(4, old_array.Get(7));
            new_array.Set(7, old_array.Get(4));

            byte lastData = 0;
            for (int i = 0; i < BYTE; ++i)
            {
                if (new_array.Get(i))
                {
                    lastData |= (byte)(1 << i);
                }
            }

            data = lastData;
        }
    #endif
    }

}

