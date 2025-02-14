// Copyright 2013-2025 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAutomationTest;

[TestClass]
public class CoordinateTest
{
    [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentException))]
    public void CoordinateInvalidParseTest()
    {
        _ = new Coordinate("bogus");
    }

    [TestMethod, TestCategory("Unit"), ExpectedException(typeof(FormatException))]
    public void CoordinateNonNumericCoordinateTest()
    {
        _ = new Coordinate("bogus,0");
    }

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
        Assert.AreEqual(new Coordinate(23, 45).GetHashCode(), coordinate.GetHashCode(), "Hash codes match");
    }
}