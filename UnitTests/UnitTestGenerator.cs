using System;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prime_Numbers_Generator;
using W3b.Sine;

namespace UnitTests
{
    [TestClass]
    public class UnitTestGenerator
    {
        [TestMethod]
        public void IsPrimeTest()
        {
            Generator gen = new Generator();

            Assert.IsTrue(gen.IsPrime(7, new BigNumDec(0.9995)));
            Assert.IsTrue(gen.IsPrime(BigInteger.Parse("900900900900990990990991")));
            Assert.IsTrue(gen.IsPrime(BigInteger.Parse("8683317618811886495518194401279999999")));

            Assert.IsFalse(gen.IsPrime(24));
            Assert.IsFalse(gen.IsPrime(7381));
            Assert.IsFalse(gen.IsPrime(15841));
            Assert.IsFalse(gen.IsPrime(BigInteger.Parse("5209156278921376498326459271950354857")));
        }

        [TestMethod]
        public void NextBigIntegerTest()
        {
            Random rand = new Random();

            byte[] aaa = new byte[] { 10, 20, 30, 40, 50 };
            byte[] bbb = aaa.Reverse().ToArray();
            bool isSqeEq = bbb.SequenceEqual(new byte[] { 50, 40, 30, 20, 10 });
            Assert.IsTrue(isSqeEq);

            BigInteger b0 = rand.NextBigInteger(0);
            BigInteger b1 = rand.NextBigInteger(1);
            BigInteger b2 = rand.NextBigInteger(2);
            BigInteger b3 = rand.NextBigInteger(100);
            BigInteger b4 = rand.NextBigInteger(BigInteger.Pow(2, 127));

            Assert.IsTrue(b0 >= 0);
            Assert.IsTrue(b0 == 0);
            Assert.IsTrue(b1 >= 0);
            Assert.IsTrue(b1 < 1);
            Assert.IsTrue(b2 >= 0);
            Assert.IsTrue(b2 < 2);
            Assert.IsTrue(b3 >= 0);
            Assert.IsTrue(b3 < 100);
            Assert.IsTrue(b4 >= 0);
            Assert.IsTrue(b4 < BigInteger.Pow(2, 130));

            try
            {
                BigInteger b5 = rand.NextBigInteger(-BigInteger.Pow(2, 127));
                Assert.Fail();
            }
            catch (Exception ex) { }
        }

        [TestMethod]
        public void GeneratorTest()
        {
            Generator gen = new Generator();

            try
            {
                var res = gen.Next(1, 80, new BigNumDec(2e-80));
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }
    }
}
