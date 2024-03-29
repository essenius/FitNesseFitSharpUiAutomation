﻿// Copyright 2013-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;
using UiAutomation.Patterns;

namespace UiAUtomationTest
{
    [TestClass]
    public class PatternTest
    {
        [TestMethod]
        [TestCategory("Experiments")]
        public void PatternTest1()
        {
            var fixture = new UiAutomationFixture();
            try
            {
                UiAutomationFixture.TimeoutSeconds = 1;
                UiAutomationFixture.SearchBy("Name");
                Assert.IsTrue(fixture.StartApplication("notepad.exe"), "Notepad started");
                var control = fixture.GetControl("classname:edit");
                var pattern = new LegacyIAccessiblePattern(control.AutomationElement);
                Assert.AreEqual(SetResult.Success, pattern.TrySet("hello"));
                Assert.IsTrue(pattern.TryGet(out var content));
                Assert.AreEqual("hello", content);
            }
            finally
            {
                fixture.ForcedCloseApplication();
            }
        }
    }
}
