using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Utils.Tests
{
    [TestFixture]
    public class RangeParserText
    {
        [TestCase("0-10", 0, 10)]
        [TestCase("10-20", 10, 20)]
        [TestCase("1000-2000", 1000, 2000)]
        public void ValidRange(string source, int min, int max)
        {
            var result = RangeParser.Parse(source);
            Assert.AreEqual(result.Start.Value, min);
            Assert.AreEqual(result.End.Value, max);
        }

        [TestCase("")]
        [TestCase("string")]
        [TestCase("10-")]
        [TestCase("-10")]
        [TestCase("test-")]
        [TestCase("-test")]
        [TestCase("10-test")]
        [TestCase("test-10")]
        [TestCase("10.00-")]
        [TestCase("-10.00")]
        [TestCase("10.00-20")]
        [TestCase("20-10.00")]
        [TestCase("10.00-20.00")]
        public void InvalidRange(string source)
        {
            Assert.Throws<ArgumentException>(() => RangeParser.Parse(source));
        }
    }
}
