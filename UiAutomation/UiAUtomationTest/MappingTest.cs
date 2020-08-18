// Copyright 2013-2020 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class MappingTest
    {
        [TestMethod, TestCategory("Unit"), ExpectedException(typeof(ArgumentNullException)),
         SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "expecting exception")]
        public void MappingInvalidSerializationTest()
        {
            var map = new Mapping<int>("test");
            var sc = new StreamingContext();
            map.GetObjectData(null, sc);
        }

        [TestMethod, TestCategory("Unit")]
        public void MappingSerializeTest()
        {
            var map = new Mapping<int>("Number")
            {
                {"One", 1},
                {"Two", 2}
            };

            IFormatter formatter = new BinaryFormatter();
            Stream writeStream = new FileStream("mapping.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(writeStream, map);
            writeStream.Close();

            Stream readStream = new FileStream("mapping.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            var deserializedMap = (Mapping<int>) formatter.Deserialize(readStream);
            readStream.Close();

            Assert.AreEqual(1, deserializedMap.Map("One"), "Mapping One");
            Assert.AreEqual(2, deserializedMap.Map("Two"), "Mapping Two");
            try
            {
                deserializedMap.Map("Three");
                Assert.Fail("No exception raised mapping Three");
            }
            catch (ArgumentException exception)
            {
                Assert.AreEqual("Three is an unrecognized Number", exception.Message, "Exception message");
            }
        }
    }
}
