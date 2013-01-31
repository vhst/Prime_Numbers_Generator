using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using W3b.Sine;

namespace Prime_Numbers_Generator
{
    public class Generator
    {
        #region Const and readonly

        private const int SequenceLength = 160; // in Next(), >= 160
        private const int N = 100; // in IsPrime(), >= 50
        private static readonly BigInteger TwoinSequenceLength = BigInteger.Pow(2, SequenceLength); // in GetVk(), represent 2 in SequenceLength
        private static readonly BigInteger Twoin160 = BigInteger.Pow(2, 160); // in Next(), represent 2 in 160
        
        private readonly Random _rand;
        private readonly RNGCryptoServiceProvider _provider;
        private readonly SHA1CryptoServiceProvider _sha;

        #endregion

        #region Properties

        public int Counter { get; private set; }
        public BigInteger Seed { get; private set; }

        #endregion

        #region Constructor

        public Generator()
        {
            _rand = new Random();
            _provider = new RNGCryptoServiceProvider();
            _sha = new SHA1CryptoServiceProvider();
        }

        #endregion

        #region Private methods

        private BigInteger GetVk(BigInteger seed, int offset, int k)
        {
            byte[] hash = _sha.ComputeHash(((seed + offset + k) % TwoinSequenceLength).ToByteArray());

            return new BigInteger(hash.Reverse().Concat(new byte[] { 0 }).ToArray());
        }

        private int GetInterationsAmount(BigNumDec probability)
        {
            if (probability.Equals(0) || probability.Equals(1))
                throw new ArgumentException("Probability must be in range (0; 1)");

            int iterations = 1;
            BigNum baseNum = 0.25;
            while (true)
            {
                if (baseNum <= probability)
                    break;

                baseNum *= 0.25;
                iterations++;
            }

            return iterations;
        }

        private bool IsPrime(BigInteger number, int passes)
        {
            if (passes <= 0)
                throw new ArgumentException("Passes must be positive");

            int iterations = passes;

            while (iterations > 0)
            {
                if (!IsPrime(number))
                    return false;

                --iterations;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Generates 2 prime numbers 
        /// </summary>
        /// <param name="n">Number to generate L where L - 1 = B * 160 + N. This value must be in range between 0 and 160</param>
        /// <param name="b">Number to generate L where L - 1 = B * 160 + N. This value must be in range between 0 and 160</param>
        /// <param name="probability"></param>
        /// <returns>Returns Tuple with 2 prime numbers where Item1 = p and Item2 = q</returns>
        public Tuple<BigInteger, BigInteger> Next(int n, int b, BigNumDec probability)
        {
            if (n > 160 || b > 160 || n < 0 || b < 0)
                throw new ArgumentException("n and b must be in range between 0 and 160");

            int L = n * 160 + b + 1;
            BigInteger twoInLminusOne = BigInteger.Pow(2, L - 1);
            int iterationsAmount = GetInterationsAmount(probability);

            while (true)
            {
                // Шаг 1. Выбираем произвольную последовательность из как минимум 160 бит и называем ее "seed". Пусть "SequenceLength" – длина "seed" в битах
                byte[] seedArray = new byte[SequenceLength >> 3];
                _provider.GetBytes(seedArray);
                byte[] positiveSeedArray = seedArray.Reverse().Concat(new byte[] { 0 }).ToArray(); // set "positive" byte
                Seed = new BigInteger(positiveSeedArray);

                // Шаг 2. Вычисляем U = SHA[SEED] XOR SHA[(SEED+1) mod 2^SequenceLength ]
                byte[] U = _sha.ComputeHash(seedArray).Xor(_sha.ComputeHash( ((Seed + 1) % BigInteger.Pow(2, SequenceLength)).ToByteArray() ) );

                // Шаг 3. Создаем q из U устанавливая младший и старший бит равным 1:
                // q = U OR 2^159 OR 1
                // Заметим, что 2^159 < q < 2^160
                byte[] qArray = (byte[]) U.Clone();
                qArray[0] |= 128;
                qArray[qArray.Length - 1] |= 1;
                byte[] positiveQArray = qArray.Reverse().Concat(new byte[] {0}).ToArray();
                BigInteger q = new BigInteger(positiveQArray);

                // Шаг 4. Проверяем q на простоту
                // IsPrime()

                // Шаг 5. Если q непростое, переходим на шаг 1
                if (!IsPrime(q, iterationsAmount))
                {
                    continue;
                }

                // Шаг 6. Пусть counter = 0 и offset = 2
                Counter = 0;
                int offset = 2;

                while (true)
                {
                    // Шаг 7. Для k = 0, ... , n вычисляем Vk = SHA[(SEED + offset + k) mod 2^g]
                    // GetVk()

                    // Шаг 8. Вычисляем W = V0 + V1 * 2^160 + ... + Vn-1 * 2^((n-1)*160) + (Vn mod 2^b) * 2^(n*160)
                    // X = W + 2^(L-1)
                    // Заметим, что 0 < = W < 2^(L-1)  и 2^(L-1) < = X < 2^L. 
                    BigInteger W = new BigInteger(0);
                    for (int i = 0; i < n; i++)
                    {
                        BigInteger Vk = GetVk(Seed, offset, i);
                        Vk *= Twoin160 * BigInteger.Pow(2, i);
                        W += Vk;
                    }
                    W += (GetVk(Seed, offset, n) % BigInteger.Pow(2, b)) * Twoin160 * BigInteger.Pow(2, n);
                    BigInteger X = W + BigInteger.Pow(2, L - 1);

                    // Шаг 9. Пусть c = X mod 2q и p = X - (c - 1). Заметим, что p равно 1 mod 2q
                    BigInteger c = X % (2 * q);
                    BigInteger p = X - c + 1;

                    // Шаг 10. Если p < 2^(L-1), тогда переходим на шаг 13
                    if (p < twoInLminusOne)
                    {
                        // Шаг 13. Counter = Counter + 1 и offset = offset + n + 1
                        Counter++;
                        offset += n + 1;

                        // Шаг 14. Если counter >= 2^12 = 4096 переходим на шаг 1, иначе переходим на шаг 7
                        if (Counter > 4096)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // Шаг 11. Проверяем p на простоту
                    if (IsPrime(p, iterationsAmount))
                    {
                        // Шаг 12. Если p прошло тест переходим на шаг 15
                        // Шаг 15. Сохраняем SEED и Counter для  того, чтобы подтвердить правильную генерацию p и q
                        return new Tuple<BigInteger, BigInteger>(p, q);
                    }
                    else
                    {
                        // Шаг 13. Counter = Counter + 1 и offset = offset + n + 1
                        Counter++;
                        offset += n + 1;

                        // Шаг 14. Если counter >= 2^12 = 4096 переходим на шаг 1, иначе переходим на шаг 7
                        if (Counter > 4096)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether BigInteger is prime number with probability that can be greater than requested probability
        /// </summary>
        /// <param name="number">The BigInteger to check</param>
        /// <param name="probability">Probability of error</param>
        /// <returns></returns>
        public bool IsPrime(BigInteger number, BigNumDec probability)
        {
            if (probability.Equals(0) || probability.Equals(1))
                throw new ArgumentException("Probability must be in range (0; 1)");

            int iterations = 1;
            BigNum baseNum = 0.25;
            while (true)
            {
                if (baseNum <= probability)
                    break;

                baseNum *= 0.25;
                iterations++;
            }

            while (iterations > 0)
            {
                if (!IsPrime(number))
                    return false;

                --iterations;
            }

            return true;
        }

        /// <summary>
        /// Determines whether BigInteger is prime number with probability 0.25
        /// </summary>
        /// <param name="number">The BigInteger to check</param>
        /// <returns></returns>
        public bool IsPrime(BigInteger number)
        {
            if (number < 0)
                throw new ArgumentException("Number must be positive");

            if (number.IsEven)
            {
                return false;
            }

            // Шаг 1. Устанавливаем i = 1 и выбираем n >= 50
            int i = 1;

            // Шаг 2. Приравниваем w тестируемому числу и представляем его в виде w = 1 + 2^a * m, где m – нечетное число
            BigInteger w = number;
            --number;
            int a = 0;
            while (number.IsEven)
            {
                number >>= 1;
                a++;
            }
            BigInteger m = number;

            while (true)
            {
                // Шаг 3. Генерируем случайное число b: 1 < b < w
                BigInteger b = _rand.NextBigInteger(w);
                b = b <= 1 ? 2 : b;

                // Шаг 4. Устанавливаем j = 0 и z = b^m mod w
                int j = 0;
                BigInteger z = BigInteger.ModPow(b, m, w);

                while (true)
                {
                    // Шаг 5. Если j = 0 и z = 1, или если z = w - 1, то переходим на шаг 9
                    if ((j == 0 && z == 1) || (z == w - 1))
                    {
                        // Шаг 9. Если i < n, то устанавливаем i = i + 1 и переходим на шаг 3. Иначе, возможно w – простое число
                        if (i < N)
                        {
                            i++;
                            break;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    // Шаг 6. Если j > 0 и z = 1, то переходим на шаг 8
                    if (j > 0 && z == 1)
                    {
                        // Шаг 8. w не простое. Стоп
                        return false;
                    }

                    // Шаг 7. j = j + 1. Если j < a, то устанавливаем z = z^2 mod w и переходим на шаг 5
                    j++;
                    if (j < a)
                    {
                        z = z * z % w;
                        continue;
                    }
                    else
                    {
                        // Шаг 8. w не простое. Стоп
                        return false;
                    }
                }
            }
        }
    }
}
