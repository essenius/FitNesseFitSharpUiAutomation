using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAUtomationTest
{
    [TestClass]
    public class CoordinateTest
    {
        [TestMethod, TestCategory("Unit")]
        public void CoordinateParseTest()
        {
            var coordinate = Coordinate.Parse(string.Empty);
            Assert.AreEqual(0, coordinate.X, "X coordinate is 0");
            Assert.AreEqual(0, coordinate.Y, "Y coordinate is 0");
            Assert.AreEqual("0, 0", coordinate.ToString(), "ToString is 0, 0");
            Assert.IsFalse(coordinate.Equals(null), "coordinate is not null");
            coordinate = Coordinate.Parse(" 23 , 45 ");
            Assert.IsTrue(new Coordinate(23, 45).Equals(coordinate));
            Assert.IsFalse(new Coordinate(23, 44).Equals(coordinate));
            Assert.IsFalse(new Coordinate(24, 45).Equals(coordinate));
            Assert.IsFalse(new Coordinate(24, 44).Equals(coordinate));
            Assert.AreEqual(new Coordinate(23,45).GetHashCode(), coordinate.GetHashCode(), "Hash codes match");
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
        public void CoordinateInvalidParseTest()
        {
            var _ = new Coordinate("bogus");
        }

        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(FormatException))]
        public void CoordinateNonNUmmericCoordinateTest()
        {
            var _ = new Coordinate("bogus,0");
        }
    }
}
