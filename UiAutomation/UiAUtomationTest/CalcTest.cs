// Copyright 2013-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

// This set of tests may fail if other UWP apps (such as Edge) are active during the test
namespace UiAutomationTest
{
    [TestClass]
    public class CalcTest
    {
        private static int _testCounter;
        private static UiAutomationFixture _fixture;

        [TestMethod, TestCategory("Calc")]
        public void CalcGetChildValue()
        {
            _fixture.PressKey("456");
            Assert.AreEqual("456", _fixture.FirstTextUnder(Fields.Result), "The first text control under the Result box contains 456");
            Assert.AreEqual("456", _fixture.ValueOfControl(Fields.Result));
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckDefaultResult()
        {
            var actual = _fixture.ValueOfControl(Fields.Result);
            Assert.AreEqual("0", actual, "Result is '{0}' rather than '0'", actual);
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckEnterNumber()
        {
            Assert.IsTrue(_fixture.ClickControl(Fields.One));
            Assert.IsTrue(_fixture.ClickControl(Fields.Two));
            Assert.IsTrue(_fixture.ClickControl(Fields.Three));
            Assert.IsTrue(_fixture.ClickControl(Fields.Negate));
            Assert.AreEqual("Display is -123", _fixture.NameOfControl(Fields.Result));
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckExists()
        {
            Assert.IsTrue(_fixture.ControlExists(Fields.Result), "Result control exists");
            Assert.IsFalse(_fixture.ControlExists(Fields.NonExisting), "Non-existing control does not exist");
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckGracefulHandlingOfNonexistingControl()
        {
            _fixture.SetTimeoutSeconds(1);
            Assert.IsFalse(_fixture.ClickControl(Fields.NonExisting), "Click non-existing Control");
            Assert.IsFalse(_fixture.SelectItem(Fields.NonExisting), "Select non-existing Item");
            Assert.AreEqual("null", _fixture.ValueOfControl(Fields.NonExisting), "Get value of non-existing Control");
            Assert.IsFalse(_fixture.WaitForControl(Fields.NonExisting), "Wait for non-existing control");
            Assert.IsTrue(_fixture.WaitUntilControlDisappears(Fields.NonExisting),
                "Wait for non-existing control to disappear");
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckInvalidOperations()
        {
            Assert.IsTrue(_fixture.ControlExists(Fields.One), "Control '1' exists");
            Assert.IsFalse(_fixture.ToggleControl(Fields.One), "Cannot toggle control '1'");
            Assert.IsFalse(_fixture.SetValueOfControlTo(Fields.CalculatorExpression, "M"), "Setting value of control without patterns");
            Assert.IsFalse(_fixture.SelectItem(Fields.One), "Selecting value of control without selection pattern");
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckKeyPresses()
        {
            _fixture.SetTimeoutSeconds(1);
            Assert.IsTrue(_fixture.PressKey(Win10AppKeys.ScientificMode), "Press " + Win10AppKeys.ScientificMode);
            Assert.IsTrue(_fixture.WaitForControl(Fields.Pi), "Wait for Pi");
            Assert.IsTrue(_fixture.PressKey(Win10AppKeys.StandardMode), "Press " + Win10AppKeys.StandardMode);
            Assert.IsFalse(_fixture.WaitForControl(Fields.Pi), "Wait for Pi fails");
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckScientific()
        {
            Assert.IsTrue(_fixture.ClickControl(Fields.Menu), "Click menu");
            Assert.IsTrue(_fixture.WaitForControlAndClick(Fields.Scientific), "Click Scientific menu item");

            Assert.IsTrue(_fixture.WaitForControl(Fields.Pi), "Wait for Pi");
            Assert.IsTrue(_fixture.ClickControl(Fields.Degrees), "Set Radians");
            Assert.IsTrue(_fixture.WaitForControl(Fields.Radians), "Wait for Radians");
            Assert.IsTrue(_fixture.ClickControl(Fields.One), "Push 1");
            Assert.IsTrue(_fixture.ClickControl(Fields.Sine), "Push Sine");
            Assert.AreEqual("0.8414709848078965066525023216303", _fixture.ValueOfControl(Fields.Result));
            Assert.AreEqual(@"Expression is sine radians (1)", _fixture.ValueOfControl(Fields.CalculatorExpression), "Expression");

            Assert.IsTrue(_fixture.ClickControl(Fields.Menu), "Click View menu 2nd time");
            Assert.IsTrue(_fixture.WaitForControlAndClick(Fields.Standard), "Click Standard menu item");
            Assert.IsTrue(_fixture.WaitUntilControlDisappears(Fields.Radians));
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckSimplifiedApi()
        {
            Assert.IsTrue(_fixture.ClickControl("Clear"), "Push Clear");
            Assert.IsTrue(_fixture.ClickControl("One"), "Push 1");
            Assert.IsTrue(UiAutomationFixture.SearchBy("ID"), "Search by ID");
            Assert.AreEqual("1", _fixture.ValueOfControl("CalculatorResults"), "Result is 1");
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcVolume()
        {
            Assert.IsTrue(_fixture.ClickControl(Fields.Menu), "Click menu");

            var result = _fixture.ListOfControls(@"controltype:list");
            Assert.IsTrue(result.Contains("Found 1 items"));
            Assert.IsTrue(result.Contains("Id=FlyoutNav"));
            Assert.IsTrue(_fixture.ControlIsVisible(Fields.Scientific));
            Assert.IsTrue(_fixture.WaitForControlAndClick(Fields.Volume), "Click Volume");
            Assert.IsTrue(_fixture.WaitForControl(Fields.OutputUnit), "Wait for Output Unit");
            Assert.IsTrue(_fixture.SetValueOfControlTo(Fields.OutputUnit, "Liters"), "Set Output to Liters");
            Assert.IsTrue(_fixture.WaitForControl(Fields.InputUnit), "Wait for Input Unit");
            Assert.IsTrue(_fixture.ClickControl(Fields.InputUnit), "Click InputUnit");
            Assert.IsTrue(_fixture.PressKey("{PgDn}{PgDn}"), "Press Page Down twice to get Gallons (US) in display");
            Assert.IsTrue(_fixture.SetValueOfControlTo(Fields.InputUnit, "Gallons (US)"), "Select Gallons");
            Assert.IsTrue(_fixture.WaitForControl(Fields.Input), "Wait for Input");
            Assert.IsTrue(_fixture.ClickControl(Fields.Input));
            Assert.IsTrue(_fixture.PressKey("10"));
            Assert.AreEqual("10", _fixture.ValueOfControl(Fields.Input), "Input OK");
            Assert.AreEqual("37.85412", _fixture.ValueOfControl(Fields.Output), "Output OK");
        }

        [TestMethod, TestCategory("Calc")]
        public void CalcCheckWindowValue()
        {
            UiAutomationFixture.SearchBy("ControlType");
            // also test legacy pattern get since we don't use the Window pattern yet.
            Assert.AreEqual("Calculator", _fixture.ValueOfControl("Window"), "value of Window is 'Calculator'");
        }

        [ClassCleanup]
        public static void TearDown()
        {
            Assert.IsTrue(_fixture.ForcedCloseApplication(), "Calc stopped");
        }

        [ClassInitialize]
        public static void Win10SetupCalc(TestContext testContext)
        {
            _fixture = new UiAutomationFixture();
            Assert.IsFalse(_fixture.SwitchToProcess("name:Calculator"), "Check there is no calculator running already");
            _fixture.NoAutomaticSwitchToStartedApplication();
            _fixture.SetTimeoutSeconds(5);
            _fixture.StartApplication("calc.exe"); 
            Assert.IsTrue(_fixture.WaitForProcess("name:Calculator"), "Wait for process Calculator");
            Assert.IsTrue(_fixture.SwitchToProcess("name:Calculator"), "Switch to calc app");
        }

        [TestInitialize]
        public void Win10SetUpTest()
        {
            Debug.Print("Test #" + ++_testCounter);
            Assert.IsTrue(UiAutomationFixture.SearchBy("name"));
            Assert.IsTrue(_fixture.PressKey(Win10AppKeys.StandardMode), "Switch to standard mode");
            Assert.IsTrue(_fixture.PressKey(Win10AppKeys.ClearAllInput));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Completeness")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Completeness")]
        private static class Win10AppKeys
        {
            public static string StandardMode => "%1";
            public static string ScientificMode => "%2";
            public static string ProgrammerMode => "%3";
            public static string DateCalculationMode => "%4";
            public static string ClearAllInput => "{Esc}";
            public static string ClearCurrentInput => "{Del}";
            public static string CalcHistoryToggle => "^h";
            public static string StoreInMemory => "^m";
            public static string AddToMemory => "^p";
            public static string SubtractFromMemory => "^q";
            public static string RecallFromMemory => "^r";
            public static string ClearMemory => "^l";
            public static string ClearHistory => "^D";
            public static string MoveUpInHistory => "{UP}";
            public static string MoveDownInHistory => "{DOWN}";
            public static string PlusMinus => "{F9}";
            public static string Reciprical => "r";
            public static string SquareRoot => "@";
            public static string Percent => "{%}";
            public static string Degrees => "{F3}";
            public static string Radians => "{F4}";
            public static string Grad => "{F5}";
            public static string _10X => "^g";
            public static string Cosh => "^o";
            public static string Sinh => "^s";
            public static string Tanh => "^t";
            public static string ArcSin => "S";
            public static string ArcCos => "O";
            public static string ArcTan => "T";
            public static string YRoot => "^y";
            public static string Mod => "d";
            public static string Log => "l";
            public static string Dms => "m";
            public static string Ln => "n";
            public static string Ex => "^n";
            public static string Cos => "o";
            public static string Sin => "s";
            public static string Tan => "t";
            public static string Fe => "v";
            public static string Exp => "x";
            public static string XpowY => "y";
            public static string X3 => "#";
            public static string Nfaculty => "!";
            public static string Mode => "{%}";
            public static string Dword => "{F2}";
            public static string Word => "{F3}";
            public static string Byte => "{F4}";
            public static string Hex => "{F5}";
            public static string Dec => "{F6}";
            public static string Oct => "{F7}";
            public static string Bin => "{F8}";
            public static string Qword => "{F12}";
            public static string A => "a";
            public static string B => "b";
            public static string C => "c";
            public static string D => "d";
            public static string E => "e";
            public static string RoL => "j";
            public static string RoR => "k";
            public static string Lsh => "<";
            public static string Rsh => ">";
            public static string Modulo => "%";
            public static string Or => "|";
            public static string Xor => "{^}";
            public static string Not => "~";
            public static string And => "&";
            public static string Toggle => " ";
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Completeness")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Completeness")]
        private static class Fields
        {
            public const string Pi = "id:piButton";
            public static string Result => "id:CalculatorResults";
            public static string Clear => "name:Clear";
            public static string One => "name:one";
            public static string Two => "name:two";
            public static string Three => "NamE:three";
            public static string Negate => "id:negateButton";
            public static string Menu => "name:Menu";
            public static string Scientific => "name:Scientific Calculator";
            public static string Standard => "name:Standard Calculator";
            public static string Volume => "name:Volume Converter";
            public static string OutputUnit => "id:Units1";
            public static string Output => "id:Value1";
            public static string InputUnit => "id:Units2";
            public static string Input => "id:Value2";
            public static string Degrees => "id:DegButton";
            public static string Radians => "id:RadButton";
            public static string Gradians => "id:GradButton";
            public static string Sine => "name:Sine";
            public static string CalculatorExpression => "id:CalculatorExpression";
            public static string MemoryClear => "id:ClearMemoryButton";
            public static string MemoryAdd => "name:Memory Add";
            public static string NonExisting => "name:non-existing control";
        }
    }
}