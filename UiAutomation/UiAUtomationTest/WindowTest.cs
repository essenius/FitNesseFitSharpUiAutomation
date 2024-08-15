// Copyright 2019-2024 Rik Essenius
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
using UiAutomation.Model;

namespace UiAutomationTest
{
    [TestClass]
    public class WindowTest
    {
        [TestMethod, TestCategory("Unit")]
        public void WindowTestNull()
        {
            var window = new Window(null);
            Assert.IsFalse(window.Maximize());
            Assert.IsFalse(window.Minimize());
            Assert.IsFalse(window.Move(10, 10));
            Assert.IsFalse(window.Resize(100, 100));
            Assert.IsFalse(window.IsTopmost());
        }
    }
}
