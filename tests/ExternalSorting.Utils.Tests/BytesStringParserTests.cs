using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Utils.Tests
{
    [TestFixture]
    public class BytesStringParserTests
    {
        [TestCase("0", 0)]
        [TestCase("1", 1)]
        [TestCase("12345678", 12345678)]
        [TestCase("1k", 1024L * 1)]
        [TestCase("20k", 1024L * 20)]
        [TestCase("1m", 1024L * 1024 * 1)]
        [TestCase("20m", 1024L * 1024 * 20)]
        [TestCase("1g", 1024L * 1024 * 1024 * 1)]
        public void ValidBytesValues(string inputValue, long extpectedBytes)
        {
            var actual = BytesStringParser.ParseBytesString(inputValue);
            Assert.AreEqual(extpectedBytes, actual);
        }

        [TestCase("test")]
        [TestCase("testg")]
        [TestCase("testk")]
        [TestCase("testm")]
        [TestCase("1GB")]
        [TestCase("hellog")]
        [TestCase("-1")]
        [TestCase("-10")]
        [TestCase("-10k")]
        [TestCase("-10m")]
        [TestCase("-10g")]
        public void InvalidBytesValues(string inputValue)
        {
            Assert.Throws<ArgumentException>(() => BytesStringParser.ParseBytesString(inputValue));
        }
    }
}
