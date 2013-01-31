using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Prime_Numbers_Generator
{
    public static class ExtensionBox
    {
        public static BigInteger NextBigInteger(this Random random, BigInteger maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentException("maxValue must be positive or 0");

            if (maxValue == 0)
                return new BigInteger(0);

            byte[] maxValueArray = maxValue.ToByteArray();
            maxValueArray = maxValueArray.Reverse().ToArray();
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] data = new byte[maxValueArray.Length];
            provider.GetBytes(data);
            BigInteger resInt;

            int byteNumber = 0;
            byte maxValueFirstByte = maxValueArray[byteNumber];
            byte dataFirstByte = data[byteNumber];
            while (true)
            {
                if (maxValueFirstByte > dataFirstByte)
                {
                    data[byteNumber] = dataFirstByte;
                    break;
                }
                if (maxValueFirstByte == dataFirstByte)
                {
                    if (byteNumber + 1 >= maxValueArray.Length)
                    {
                        data[byteNumber] = dataFirstByte;
                        data = data.Reverse().ToArray();
                        resInt = new BigInteger(data);

                        return (--resInt);
                    }

                    data[byteNumber] = dataFirstByte;
                    byteNumber++;
                    maxValueFirstByte = maxValueArray[byteNumber];
                    dataFirstByte = data[byteNumber];
                    continue;
                }
                dataFirstByte >>= 1;
            }

            data = data.Reverse().ToArray();
            resInt = new BigInteger(data);

            return resInt;
        }

        public static byte[] Xor(this byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
                throw new ArgumentException("Длины массивов не совпадают");

            byte[] result = new byte[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                result[i] = (byte) (second[i] ^ first[i]);
            }

            return result;
        }
    }
}
