// Copyright 2013-2024 Rik Essenius
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ImageHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiAutomation;

namespace UiAutomationTest;

[TestClass, DeploymentItem("WpfDemoApp.exe"), DeploymentItem("WpfDemoApp.exe.config")]
// Can't put deployment items on the ClassInitialize method
public class WpfDemoAppTests
{
    private const string WpfDemoAppPath = "WpfDemoApp.exe";
    private static UiAutomationFixture _fixture;

    private static int _testCounter;

    private static readonly string[,] DataGridValues =
    {
        { "100", "Done", "Create UI Automation fixture" }, { "101", "Active", "Create demo UI Automation application" },
        { "102", "Approved", "Add UI Automation fixture to application" }, { "103", "New", "Test UI Automation fixture with GridData object" },
        { "104", "Resolved", "http://localhost:8080" }
    };

    private static readonly string TempFolder = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

    [ClassCleanup]
    public static void CleanupTestSuite()
    {
        Assert.IsTrue(_fixture.CloseApplication(), "WPF Demo App stopped");
        UiAutomationFixture.TimeoutSeconds = 3;
    }

    [ClassInitialize,
     SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "False positive, prescribed parameter for class initialization")]
    public static void PrepareTestSuite(TestContext testContext)
    {
        UiAutomationFixture.TimeoutSeconds = 8;
        _fixture = new UiAutomationFixture();
        _fixture.SetAutomaticSwitchToStartedApplication();
        Assert.IsTrue(
            _fixture.StartApplicationWithWorkingFolder(WpfDemoAppPath, TempFolder),
            "WpfDemoApp started with working folder"
        );
        _fixture.WaitForControl("id:workingFolder");
        var actualWorkFolder = _fixture.ValueOfControl("id:WorkingFolder");
        Assert.AreEqual(TempFolder, actualWorkFolder, "Working folder is OK");
        Assert.IsTrue(_fixture.CloseApplication(), "WPF Demo App stopped");
        Assert.IsTrue(
            _fixture.StartApplicationWithWorkingFolder(WpfDemoAppPath, ""),
            "WpfDemoApp started with empty working folder"
        );
        Assert.AreNotEqual(TempFolder, _fixture.ValueOfControl("id:WorkingFolder"), "Working folder is OK 2");
    }

    [TestInitialize]
    public void SetUp()
    {
        Debug.Print("Test #" + ++_testCounter);
        UiAutomationFixture.SearchBy("Name");
    }

    private static void TestTable(string searchCriterion, IReadOnlyList<string> header, string[,] expectedValues)
    {
        var etv = new ExtractGrid(searchCriterion);
        var table = etv.Query();
        Assert.IsNotNull(table);
        Assert.AreEqual(expectedValues.GetLength(0), table.Count, "Row Count for {0}", searchCriterion);
        for (var row = 0; row < table.Count; row++)
        {
            var rowCollection = table[row] as Collection<object>;
            Assert.IsNotNull(rowCollection);
            Assert.AreEqual(
                expectedValues.GetLength(1),
                rowCollection.Count, "Column Count for {0}", searchCriterion
            );
            for (var column = 0; column < rowCollection.Count; column++)
            {
                var columnCollection = rowCollection[column] as Collection<object>;
                Assert.IsNotNull(columnCollection);
                Assert.AreEqual(
                    2,
                    columnCollection.Count,
                    "Cell Count for {0}({1},{2})", searchCriterion, row, column
                );
                Assert.AreEqual(
                    header[column],
                    columnCollection[0],
                    "Header for {0}({1},{2})", searchCriterion, row, column
                );
                Assert.AreEqual(
                    expectedValues[row, column],
                    columnCollection[1],
                    "value for {0}({1},{2})", searchCriterion, row, column
                );
            }
        }
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckCalendar()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:More Controls"), "Select 'More Controls' tab");
        Assert.AreEqual("", _fixture.ValueOfControl("ControlType:Calendar"), "Default value of Calendar1");
        var expectedDate = DateTime.Today;
        // select a different day in this month, so that we don't need to select a different month first
        var daysToAdd = expectedDate.Day < 15 ? 10 : -10;
        var selectionValue = expectedDate.AddDays(daysToAdd).ToLongDateString();
        // this should be set using a selection 
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("id:Calendar1", selectionValue),
            "Set value of Calendar1 to " + selectionValue
        );
        Assert.AreEqual(selectionValue, _fixture.ValueOfControl("id:Calendar1"), "New value of Calendar1");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckCheckBox()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        UiAutomationFixture.SearchBy("id");
        Assert.AreEqual("Off", _fixture.ValueOfControl("CheckBox1"), "Default value of CheckBox1");

        Assert.IsTrue(_fixture.ToggleControl("CheckBox1"), "First toggle on CheckBox1");
        Assert.AreEqual("On", _fixture.ValueOfControl("CheckBox1"), "Value of CheckBox1 after first toggle");
        Assert.AreEqual("Checked CheckBox1", _fixture.ValueOfControl("TextBlock1"));

        Assert.IsTrue(_fixture.ToggleControl("CheckBox1"), "Second toggle on CheckBox1");
        Assert.AreEqual("Off", _fixture.ValueOfControl("CheckBox1"), "Value of CheckBox1 after second toggle");
        Assert.AreEqual("Unchecked CheckBox1", _fixture.ValueOfControl("TextBlock1"));

        Assert.AreEqual(
            "Off",
            _fixture.ValueOfControl("ThreeStateCheckBox"),
            "Default Value of ThreeStateCheckBox"
        );

        Assert.IsTrue(_fixture.ToggleControl("ThreeStateCheckBox"), "First toggle on ThreeStateCheckBox");
        Assert.AreEqual(
            "On",
            _fixture.ValueOfControl("ThreeStateCheckBox"),
            "Value of ThreeStateCheckBox after first toggle"
        );
        Assert.AreEqual("Checked ThreeStateCheckBox", _fixture.ValueOfControl("TextBlock1"));

        Assert.IsTrue(_fixture.ToggleControl("ThreeStateCheckBox"), "Second toggle on ThreeStateCheckBox");
        Assert.AreEqual(
            "Indeterminate",
            _fixture.ValueOfControl("ThreeStateCheckBox"),
            "Value of ThreeStateCheckBox after second toggle"
        );

        Assert.IsTrue(_fixture.ToggleControl("ThreeStateCheckBox"), "Third toggle on ThreeStateCheckBox");
        Assert.AreEqual(
            "Off",
            _fixture.ValueOfControl("ThreeStateCheckBox"),
            "Value of ThreeStateCheckBox after third toggle"
        );
        Assert.AreEqual("Unchecked ThreeStateCheckBox", _fixture.ValueOfControl("TextBlock1"));

        Assert.IsFalse(_fixture.ToggleControl("DisabledCheckBox"), "Toggle on DisabledCheckBox");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckClickButton()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.IsTrue(_fixture.ClickControl("id:Button1"));
        Assert.AreEqual("Clicked Button1", _fixture.ValueOfControl("id:TextBlock1"));
        Assert.IsFalse(_fixture.ClickControl("id:nonexisting"));
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckComboBox()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.AreEqual("", _fixture.ValueOfControl("id:ComboBox1"), "Default value of ComboBox1");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:ComboBox1", "ComboItem2"), "Set value of ComboBox1");
        Assert.AreEqual("ComboItem2", _fixture.ValueOfControl("id:ComboBox1"), "New value of ComboBox1");
        Assert.AreEqual("Selected ComboBox1 item ComboItem2", _fixture.ValueOfControl("id:TextBlock1"));
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:ComboBox1", ""), "Set value of ComboBox1");
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("id:ComboBox1"), "New value of ComboBox1 is empty");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckDatePicker()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:More Controls"), "Select 'More Controls' tab");
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("id:DatePicker1", "23-02-2014 16:10"),
            "Set value of DatePicker1"
        );
        Assert.AreEqual(
            "23-Feb-2014 16:10:00",
            _fixture.ValueOfControl("id:DatePicker1"),
            "New value of DatePicker1"
        );
        Assert.AreEqual(
            "23-Feb-2014",
            _fixture.ValueOfControl("id:DatePickerTextBlock"),
            "New value of DatePickerTextBlock"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckDragDrop()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.IsTrue(_fixture.SelectItem("Caption:Drag Drop"), "Select 'Drag Drop' tab");
        Assert.IsTrue(_fixture.DragControlAndDropOnControl("id:DragFrom", "id:DropTo"), "DragFrom DropTo");
        Assert.AreEqual(
            "...",
            _fixture.ValueOfControl("id:DragFrom"),
            "Source value changed"
        );
        Assert.AreEqual("Drag from here", _fixture.ValueOfControl("id:DropTo"), "Target value changed");

        // test used LightSeaGreen first, but that is not visible and therefore fails in Win 2012
        Assert.IsTrue(_fixture.DragControl("id:PapayaWhipItem"), "Drag PapayaWhip");
        Assert.IsTrue(_fixture.DropOnControl("id:ColorDropTextBlock"), "Drop on ColorDropTextBlock");
        Assert.AreEqual(
            "Color is PapayaWhip",
            _fixture.ValueOfControl("id:ColorDropTextBlock"),
            "Color has been dropped"
        );

        Assert.IsTrue(_fixture.DragControlAndDropOnControl("id:DragFrom", "id:ColorDropTextBlock"));
        Assert.AreEqual(
            "Could not convert '...' into a color",
            _fixture.ValueOfControl("id:ColorDropTextBlock"),
            "Color has been dropped"
        );

        Assert.IsTrue(
            _fixture.DragControlAndDropOnControl("name:PapayaWhip", "id:DragFrom"),
            "drag from color and drop on label - but does nothing"
        );
        Assert.AreEqual("...", _fixture.ValueOfControl("id:dragFrom"), "Drop attempt didn't change value");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckExpandCollapse()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.IsTrue(UiAutomationFixture.SearchBy("Caption"), "Default search is by Caption");
        Assert.IsTrue(_fixture.SelectItem("Tree View"), "Select 'Tree View' tab");
        Assert.IsFalse(_fixture.ControlExists("Step 2"), "Step 2 tree item does not exist yet");
        Assert.IsTrue(_fixture.ExpandControl("ID:TreeView1"), "Expanding TreeView1 succeeds");
        Assert.IsTrue(_fixture.ExpandControl("Processes"), "Expand Processes");
        Assert.IsTrue(_fixture.SetFocusToControl("Main Process 1"));
        Assert.IsTrue(_fixture.ExpandControl("Main Process 1"), "Expand Main Process 1");
        Assert.IsTrue(_fixture.SetFocusToControl("Sub Process 1"));
        Assert.IsTrue(_fixture.ExpandControl("Sub Process 1"), "Expand Sub Process 1");
        Assert.IsTrue(_fixture.SetFocusToControl("Step 2"));
        Assert.IsTrue(_fixture.ControlExists("Step 2"), "Step 2 tree item now exists");
        Assert.IsTrue(_fixture.ControlIsVisible("Step 2"), "Step 2 tree item is visible");
        Assert.IsTrue(_fixture.ExpandControl("Products"), "Expanding already expanded control is OK");
        Assert.IsTrue(_fixture.ControlIsVisible("Core Product 1"), "Core Product 1 tree item is still visible");
        Assert.IsTrue(_fixture.CollapseControl("Processes"), "Collapsing Processes tree item");
        Assert.IsFalse(_fixture.ControlIsVisible("Step 2"), "Step 2 tree item is now invisible");
        Assert.IsTrue(_fixture.CollapseControl("ID:TreeView1"), "Collapsing TreeView1");
        Assert.IsFalse(_fixture.ControlIsVisible("Core Product 1"), "Core Product 1 tree item is now invisible");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckGrid()
    {
        const string dataGrid = "DataGrid1";
        Assert.IsTrue(UiAutomationFixture.SearchBy("id"), "Default search is by ID");
        Assert.IsTrue(_fixture.SelectItem("Caption:Data Grid"), "Select 'Data Grid' tab");
        Assert.AreEqual(5, _fixture.RowCountOfControl(dataGrid), "Get Row Count");
        Assert.AreEqual(3, _fixture.ColumnCountOfControl(dataGrid), "Get Column Count");
        Assert.AreEqual(
            "row 2, column 3",
            _fixture.CellInControlContaining(dataGrid, "Create demo UI Automation application").ToString(),
            "Get row with cell 'Demo UI...'"
        );
        Assert.AreEqual(
            "row 3, column 2",
            _fixture.CellInControlContaining("DataGrid1", "Approved").ToString(),
            "Get row with cell 'Approved'"
        );
        Assert.IsNull(_fixture.CellInControlContaining(dataGrid, "Non-existing value"), "Search non-existent cell");
        Assert.IsNull(_fixture.CellInControlContaining("NoGrid", "Non-existing value"), "Search non-existing grid");

        var item = _fixture.CellInControlContaining(dataGrid, "101");
        Assert.AreEqual(2, item.Row, "Row OK");
        Assert.AreEqual(1, item.Column, "Column OK");
        var locator = $"{dataGrid}[{item}]";
        Assert.AreEqual("101", _fixture.ValueOfControl(locator), "Contains 101");
        Assert.AreNotEqual("101", _fixture.ValueOfControl("GridTextbox"), "Initial value of GridBox != 101");
        Assert.IsTrue(_fixture.DoubleClickControl(locator), "DoubleClick 101");
        Assert.AreEqual("101", _fixture.ValueOfControl("GridTextbox"), "GridBox contains 101");
        Assert.IsTrue(_fixture.DoubleClickControl($"{dataGrid}[row 3, col 2]"), "DoubleClick row 3 column 2");
        Assert.IsTrue(_fixture.ClickControl($"{dataGrid} [4,3]"), "Click row 4 column 3");
        Assert.AreEqual(
            "row 4, column 3",
            _fixture.SelectedCellInControl(dataGrid).ToString(),
            "selected cell is 4,3"
        );
        Assert.AreEqual("Approved", _fixture.ValueOfControl("GridTextbox"), "GridBTextBox contains Approved");
        Assert.IsTrue(_fixture.ClickControl($"{dataGrid}[row 2]"), "Click Row 2");
        Assert.AreEqual(
            "row 2, column 1",
            _fixture.SelectedCellInControl("DataGrid1").ToString(),
            "Selected cell returns value of first column"
        );
        Assert.IsTrue(_fixture.ClickControl($"{dataGrid}[col 2]"), "Click Header 2");
        Assert.AreEqual("Active", _fixture.ValueOfControl($"{dataGrid}[1,2]"), "Clicking header sorts column");

        Assert.IsTrue(_fixture.ClickControl($"{dataGrid}[column 1]"));
        Assert.AreEqual("100", _fixture.ValueOfControl($"{dataGrid}[row 1, column 1]"));

        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.AreEqual(0, _fixture.RowCountOfControl("Button1"), "Buttons don't have rows");
        Assert.AreEqual(0, _fixture.ColumnCountOfControl("MultiValueListBox"), "ListBoxes don't have rows");
        Assert.IsNull(
            _fixture.CellInControlContaining("TreeView1", "Core Product 1"),
            "TreeViews don't have cells"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckListBox()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.AreEqual("", _fixture.ValueOfControl("id:ListBox1"), "Default value of ListBox1");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:ListBox1", "ListBoxItem3"));
        Assert.AreEqual("ListBoxItem3", _fixture.ValueOfControl("id:ListBox1"), "New value of ListBox1");
        Assert.AreEqual("Selected ListBox1 item ListBoxItem3", _fixture.ValueOfControl("id:TextBlock1"));
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:ListBox1", "ListBoxItem5"));
        Assert.AreEqual("ListBoxItem5", _fixture.ValueOfControl("id:ListBox1"), "New value of ListBox1");
        Assert.IsFalse(_fixture.SetValueOfControlTo("id:ListBox1", "non-existing value"));
        Assert.AreEqual("ListBoxItem5", _fixture.ValueOfControl("id:ListBox1"), "Value of ListBox1 stays");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:ListBox1", ""));
        Assert.AreEqual("", _fixture.ValueOfControl("id:ListBox1"), "New value of ListBox1 is empty");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckMultiSelect()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        Assert.AreEqual(
            "MultiValueListBoxItem3;MultiValueListBoxItem5",
            _fixture.ValueOfControl("id:MultiValueListBox"),
            "Default value of MultiValueListBox"
        );
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:MultiValueListBox", ""), "Clearing value");
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("id:MultiValueListBox", "MultiValueListBoxItem1"),
            "Setting value 1"
        );
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("id:MultiValueListBox", "MultiValueListBoxItem2"),
            "Setting value 2"
        );
        Assert.AreEqual(
            "MultiValueListBoxItem1;MultiValueListBoxItem2",
            _fixture.ValueOfControl("id:MultiValueListBox"),
            "New value of MultiValueListBox"
        );
        Assert.IsTrue(_fixture.SetValueOfControlTo("Caption:MultiValueListBoxItem2", ""), "Clearing value 2");
        Assert.AreEqual(
            "MultiValueListBoxItem1",
            _fixture.ValueOfControl("id:MultiValueListBox"),
            "Value of MultiValueListBox after clearing value 2"
        );
        Assert.IsFalse(
            _fixture.SetValueOfControlTo("Caption:MultiValueListBoxItem3", "wrong value"),
            "Setting invalid selection item value"
        );
        Assert.AreEqual(
            "MultiValueListBoxItem1",
            _fixture.ValueOfControl("id:MultiValueListBox"),
            "Value of MultiValueListBox did not change after wring value for 2"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckPasswordBox()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:More Controls"), "Select 'More Controls' tab");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:PasswordBox1", "Secret123"), "Set value of Password");
        Assert.AreEqual(
            "Secret123",
            _fixture.ValueOfControl("id:PasswordBoxTextBlock"),
            "New value of PasswordBoxTextBlock"
        );
        // This is different in .Net 5.0 than in .Net Framework
        Assert.AreEqual("", _fixture.ValueOfControl("id:PasswordBox1"), "New value of Password");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckRadioButtons()
    {
        Assert.IsTrue(UiAutomationFixture.SearchBy("Caption"));
        Assert.IsTrue(_fixture.SelectItem("Usual Controls"), "Select 'Usual Controls' tab");
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton1"));
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton2"));
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton3"));

        Assert.IsTrue(_fixture.SelectItem("RadioButton1"), "Select RadioButton1");
        Assert.AreEqual("Selected", _fixture.ValueOfControl("RadioButton1"));
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton2"));
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton3"));
        Assert.AreEqual("Checked RadioButton1", _fixture.ValueOfControl("id:TextBlock1"));

        Assert.IsTrue(_fixture.SelectItem("RadioButton2"), "Select RadioButton2");
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton1"));
        Assert.AreEqual("Selected", _fixture.ValueOfControl("RadioButton2"));
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton3"));
        Assert.AreEqual("Checked RadioButton2", _fixture.ValueOfControl("id:TextBlock1"));

        Assert.IsTrue(_fixture.SelectItem("RadioButton3"), "Select RadioButton3");
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton1"));
        Assert.AreEqual(string.Empty, _fixture.ValueOfControl("RadioButton2"));
        Assert.AreEqual("Selected", _fixture.ValueOfControl("RadioButton3"));
        Assert.AreEqual("Checked RadioButton3", _fixture.ValueOfControl("id:TextBlock1"));
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckRichTextBox()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Text Controls"), "Select 'Text Controls' tab");
        Assert.AreEqual(
            "Rich Text Box with a bit of additional text to make sure that the scroll bar is showing",
            _fixture.ValueOfControl("id:RichTextBox1"),
            "Default value of RichTextBox1"
        );
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("id:RichTextBox1", "FitNesse"),
            "Set value of RichTextBox1 to expanded child"
        );
        Assert.AreEqual("FitNesse", _fixture.ValueOfControl("id:RichTextBox1"), "New value of RichTextBox1");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckScrollBar()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:More Controls"), "Select 'More Controls' tab");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:ScrollBar1", "28"), "Set value of ScrollBar1");
        Assert.AreEqual("28", _fixture.ValueOfControl("id:ScrollBar1"), "New value of ScrollBar1");
        Assert.AreEqual("72", _fixture.ValueOfControl("id:ProgressBar1"), "New value of ProgressBar1");
        Assert.AreEqual("72", _fixture.ValueOfControl("id:Slider1"), "New value of Slider1");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckSlider()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:More Controls"), "Select 'More Controls' tab");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:Slider1", "23"), "Set value of Slider1");
        Assert.AreEqual("23", _fixture.ValueOfControl("ControlType:Slider"), "New value of Slider1");
        Assert.AreEqual("23", _fixture.ValueOfControl("id:ProgressBar1"), "New value of ProgressBar1");
        Assert.AreEqual("77", _fixture.ValueOfControl("id:ScrollBar1"), "New value of ScrollBar1");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckStatusBar()
    {
        Assert.IsTrue(_fixture.SelectItem("Usual Controls"), "Select 'Usual Controls' tab");

        Assert.AreEqual(
            "Status Bar for WPF Demo App",
            _fixture.ValueOfControl("ControlType:StatusBar"),
            "Status Bar contains the right value"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckTextBox()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Text Controls"), "Select 'Text Controls' tab");
        Assert.AreEqual("TextBox", _fixture.ValueOfControl("id:TextBox1"), "Default value of TextBox1");
        Assert.IsTrue(_fixture.SetValueOfControlTo("id:TextBox1", "Enterprise"));
        Assert.AreEqual("Enterprise", _fixture.ValueOfControl("id:TextBox1"), "New value of TextBox1");
        Assert.AreEqual(
            "Disabled TextBox",
            _fixture.ValueOfControl("id:DisabledTextBox"),
            "Default value of DisabledTextBox"
        );
        Assert.IsFalse(_fixture.SetValueOfControlTo("id:DisabledTextBox", "Should Not Work"));
        Assert.AreEqual(
            "Disabled TextBox",
            _fixture.ValueOfControl("id:DisabledTextBox"),
            "DisabledTextBox should not be changed"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoCheckTreeView()
    {
        Assert.IsTrue(_fixture.SelectItem("caption:Tree View"), "Select 'Tree View' tab");
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("ControlType:Tree", "Core Product 3"),
            "Set value of TreeView1 to expanded child"
        );
        Assert.AreEqual("Core Product 3", _fixture.ValueOfControl("id:TreeView1"), "New value of TreeView1");
        Assert.IsTrue(_fixture.ClickControl("id:TreeViewButton"), "Push Select Core Product 1 button");
        Assert.AreEqual(
            "Core Product 1",
            _fixture.ValueOfControl("id:TreeView1"),
            "Value of TreeView1 after push button"
        );
        // this is working differently in Windows 7 and 2012: 7 does the expansion implicitly. 
        Assert.IsTrue(_fixture.SetFocusToControl("Main Process 1"), "Set Focus to Main Process 1");
        Assert.IsTrue(_fixture.ExpandControl("Main Process 1"), "Expand Main Process 1");
        Assert.IsTrue(_fixture.SetFocusToControl("Sub Process 1"), "Set Focus to Sub Process 1");
        Assert.IsTrue(_fixture.ExpandControl("Sub Process 1"), "Expand Sub Process 1");
        Assert.IsTrue(_fixture.SetFocusToControl("Step 3"), "Set Focus to Step 3");
        Assert.IsTrue(_fixture.SetFocusToControl("Step 2"), "Set focus to Step 2");
        Assert.IsTrue(
            _fixture.SetValueOfControlTo("id:TreeView1", "Step 2"),
            "Set value of TreeView1 to collapsed child"
        );
        Assert.AreEqual(
            "Step 2",
            _fixture.ValueOfControl("id:TreeView1"),
            "Value of TreeView1 after set value to Scrum"
        );
        Assert.IsTrue(_fixture.ClickControl("id:ShowTreeInTextBlockButton"), "Push Show Tree In TextBlock button");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoControlGetAllPropertiesTest()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Data Grid"), "Select 'Data Grid' tab");
        var control = _fixture.GetControl("id:DataGrid1");
        Assert.AreEqual(
            "DataGrid1",
            control.Property("30011"),
            "Find property by constant value (UIA_AutomationIdPropertyId)"
        );
        Assert.AreEqual(
            "DataGrid",
            control.Property("Class Name"),
            "Find property by name (UIA_ClassNamePropertyId)"
        );
        Assert.AreEqual(5, control.Property("Row Count"), "row count");
        Assert.AreEqual(3, control.Property("Column Count"), "column count");
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoControlSearchOnBooleanCriteriaTest()
    {
        UiAutomationFixture.SearchBy("Tooltip");
        Assert.IsTrue(_fixture.SelectItem("Caption:Usual Controls"), "Select 'Usual Controls' tab");
        var control1 = _fixture.GetControl("ControlType:CheckBox && IsEnabled:false");
        Assert.IsNotNull(control1.AutomationElement, "Found a disabled checkbox");
        Assert.AreEqual(
            "Disabled CheckBox",
            _fixture.NameOfControl("ControlType:CheckBox && IsEnabled:false"),
            "Found right (only) disabled control"
        );
        Assert.AreEqual(
            "DisabledCheckBox",
            control1.AutomationElement.CurrentAutomationId,
            "Found right (only) disabled control"
        );
        Assert.IsTrue(_fixture.SelectItem("Caption:More Controls"), "Select 'More Controls' tab");
        var control2 = _fixture.GetControl("IsPassword:true");
        Assert.IsNotNull(control2.AutomationElement, "Found a password control");
        Assert.AreEqual(
            "PasswordBox1",
            control2.AutomationElement.CurrentAutomationId,
            "Found right (only) password control"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoControlSearchOnHelpTextTest()
    {
        Assert.IsTrue(_fixture.SelectItem("caption:Usual Controls"), "Select 'Usual Controls' tab");
        UiAutomationFixture.SearchBy("Tooltip");
        var control1 = _fixture.GetControl("HelpText:Checkbox with two states");
        Assert.IsNotNull(control1, "Found a control with two states");
        Assert.AreEqual(
            "CheckBox1",
            control1.AutomationElement.CurrentAutomationId,
            "Found right two state checkbox"
        );
        var control2 = _fixture.GetControl("HelpText:CheckBox with three states");
        Assert.IsNotNull(control2, "Found a control with three states");
        Assert.AreEqual(
            "ThreeStateCheckBox",
            control2.AutomationElement.CurrentAutomationId,
            "Found right three state checkbox"
        );
        var control3 = _fixture.GetControl("HelpText:");
        Assert.IsNotNull(control3.AutomationElement, "Found a control without helptext");
        Assert.IsFalse(
            string.IsNullOrEmpty(control3.AutomationElement.CurrentClassName),
            "ClassName is not null or empty"
        );
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoExtractGridWithHeadersTest()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Data Grid"), "Select data grid tab");
        Assert.AreEqual(3, _fixture.PropertyOfControl("Column Count", "id:DataGrid1"), "Column count is 3");
        Assert.AreEqual(5, _fixture.PropertyOfControl("Row Count", "id:DataGrid1"), "Row count is 4");
        TestTable("id:DataGrid1", ["Id", "Status", "Title"], DataGridValues);
    }

    [TestMethod, TestCategory("DemoApp")]
    public void WpfDemoExtractGridWithoutHeadersTest()
    {
        Assert.IsTrue(_fixture.SelectItem("Caption:Data Grid"), "Select data grid tab");
        Assert.AreEqual(
            1,
            _fixture.PropertyOfControl("ToggleState", "id:DataGridHeaderCheckbox"),
            "ToggleState is on before"
        );
        Assert.IsTrue(_fixture.ToggleControl("id:DataGridHeaderCheckbox"), "Toggle checkbox 1");
        Assert.AreEqual(
            0,
            _fixture.PropertyOfControl("Toggle State", "id:DataGridHeaderCheckbox"),
            "ToggleState is off after"
        );
        TestTable("id:DataGrid1", ["Column 1", "Column 2", "Column 3"], DataGridValues);
        Assert.IsTrue(_fixture.ToggleControl("id:DataGridHeaderCheckbox"), "Toggle checkbox 2");
        Assert.AreEqual(
            1,
            _fixture.PropertyOfControl("ToggleState", "id:DataGridHeaderCheckbox"),
            "ToggleState is on after"
        );
    }

    [TestMethod, TestCategory("DemoApp"), DeploymentItem("DemoApp-400x140-8.base64")]

    public void WpfDemoSnapshot()
    {
        Assert.IsTrue(_fixture.SelectItem("caption:Usual Controls"), "Select 'Usual Controls' tab");
        var originalSize = _fixture.WindowSize;
        var desiredSize = new Coordinate(416, 156);
        Assert.IsTrue(_fixture.ResizeWindowTo(desiredSize), "Resize succeeds");
        var stopwatch = Stopwatch.StartNew();

        // The outer pixels are not part of the window, but are added by the window manager for e.g. the glass effect. We don't want that as it is not predictable 
        var snapshot = _fixture.WindowSnapshotObjectMinusOuterPixels(8);
        Console.WriteLine($@"Snapshot1:  @ {stopwatch.Elapsed}");
        // read a base64 string from a file and decode it
        var expected = Snapshot.Parse(File.ReadAllText("DemoApp-400x140-8.base64"));
        Console.WriteLine($@"Parse:      @ {stopwatch.Elapsed}");
        var similarity = expected.SimilarityTo(snapshot);
        Console.WriteLine($@"Similarity: {similarity} @ {stopwatch.Elapsed}");
        Console.WriteLine($@"Snapshot:   {snapshot.Rendering}");
        Assert.IsTrue(similarity > 0.7, $"Snapshot similarity {similarity} > 0.7");

        var snapshot2 = _fixture.WindowSnapshot();
        Console.WriteLine($@"Snapshot2:  {snapshot2[..47]}@ {stopwatch.Elapsed}");
        // strip off the <data:image/png;base64, and the trailing ">
        var index = snapshot2.IndexOf(";base64,", StringComparison.Ordinal) + 8;
        var snapshotObject2 = Snapshot.Parse(snapshot2[index..^4]);

        var snapshot3 = _fixture.WindowSnapshotMinusOuterPixels(0);
        Console.WriteLine($@"Snapshot3:  {snapshot3[..47]} @ {stopwatch.Elapsed}");
        // do the same stripping
        index = snapshot3.IndexOf(";base64,", StringComparison.Ordinal) + 8;
        var snapshotObject3 = Snapshot.Parse(snapshot3[index..^4]);
        var similarity2 = snapshotObject2.SimilarityTo(snapshotObject3);
        Console.WriteLine($@"Similarity: {similarity2} @ {stopwatch.Elapsed}");
        Assert.IsTrue(similarity2 > 0.95, $"Snapshot similarity {similarity2} > 0.99");
        _fixture.ResizeWindowTo(originalSize);
        stopwatch.Stop();
    }
}