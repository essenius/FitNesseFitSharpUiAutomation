using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfDemoApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ListBox _colorDragSource;

        public MainWindow()
        {
            InitializeComponent();
            DataGrid1.ItemsSource = LoadCollectionData();
            WorkingFolder.Text = Directory.GetCurrentDirectory();
        }

        private static string AllTreeListItems(IEnumerable itemCollection) => TreeListItems(itemCollection, 0);

        private static string GetDataFromListBox(ItemsControl source, Point point)
        {
            if (!(source.InputHitTest(point) is UIElement element)) return null;
            var data = DependencyProperty.UnsetValue;
            while (data == DependencyProperty.UnsetValue)
            {
                Debug.Assert(element != null, "element != null");
                data = source.ItemContainerGenerator.ItemFromContainer(element);
                if (data == DependencyProperty.UnsetValue)
                {
                    element = VisualTreeHelper.GetParent(element) as UIElement;
                }
                if (Equals(element, source))
                {
                    return null;
                }
            }
            if (data == DependencyProperty.UnsetValue) return null;
            return data is ListBoxItem listboxItem ? listboxItem.Content.ToString() : string.Empty;
        }

        private static IEnumerable<WorkItem> LoadCollectionData()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem
                {
                    Id = 100,
                    Title = "Create UI Automation fixture",
                    Status = "Done"
                },
                new WorkItem
                {
                    Id = 101,
                    Title = "Create demo UI Automation application",
                    Status = "Active"
                },
                new WorkItem
                {
                    Id = 102,
                    Title = "Add UI Automation fixture to application",
                    Status = "Approved"
                },
                new WorkItem
                {
                    Id = 103,
                    Title = "Test UI Automation fixture with GridData object",
                    Status = "New"
                },
                new WorkItem
                {
                    Id = 104,
                    Title = "http://localhost:8080",
                    Status = "Resolved"
                }
            };
            return workItems;
        }

        private static string TreeListItems(IEnumerable items, int depth)
        {
            var returnValue = string.Empty;
            foreach (var tli in items.OfType<TreeViewItem>())
            {
                returnValue += string.Concat(Enumerable.Repeat(" ", depth)) + tli.Header + "\n";
                if (tli.HasItems)
                {
                    returnValue += TreeListItems(tli.Items, depth + 1);
                }
            }
            return returnValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetTreeViewItem();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Clicked Button1";
        }

        private void CheckBox1_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Checked CheckBox1";
        }

        private void CheckBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Unchecked CheckBox1";
        }

        private void ColorListBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = (ListBox) sender;
            _colorDragSource = parent;
            object data = GetDataFromListBox(_colorDragSource, e.GetPosition(parent));

            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ComboBox1.SelectedItem as ComboBoxItem;
            TextBlock1.Text = "Selected ComboBox1 item " + (item == null ? "none" : item.Content);
        }

        private void DatePicker1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker1.SelectedDate != null)
            {
                DatePickerTextBlock.Text = DatePicker1.SelectedDate.Value.ToShortDateString();
            }
        }

        private void Label1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var labelFrom = e.Source as Label;
            Debug.Assert(labelFrom != null, "labelFrom != null");
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(labelFrom, labelFrom.Content, DragDropEffects.Copy);
            }
        }

        private void Label1_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            var labelFrom = e.Source as Label;
            Debug.Assert(labelFrom != null, "labelFrom != null");
            if (!e.KeyStates.HasFlag(DragDropKeyStates.LeftMouseButton))
            {
                labelFrom.Content = "...";
            }
        }

        private void Label2_Drop(object sender, DragEventArgs e)
        {
            var labelContent = "couldn't understand what to drop";
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                labelContent = (string) e.Data.GetData(DataFormats.StringFormat);
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files) labelContent = files[0];
            }
            if (e.Source is Label labelTo) labelTo.Content = labelContent;
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ListBox1.SelectedItem as ListBoxItem;
            TextBlock1.Text = "Selected ListBox1 item " + (item == null ? "none" : item.Content);
        }

        private void PasswordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBoxTextBlock.Text = PasswordBox1.Password;
        }

        private void ProgressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressBarTextBlock.Text = Convert.ToInt32(ProgressBar1.Value).ToString(CultureInfo.InvariantCulture);
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Checked RadioButton1";
        }

        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Checked RadioButton2";
        }

        private void RadioButton3_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Checked RadioButton3";
        }

        private void ScrollBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var reverseValue = ScrollBar1.Maximum - (ScrollBar1.Value - ScrollBar1.Minimum);
            Slider1.Value = reverseValue;
            ScrollBarTextBlock.Text = Convert.ToInt32(ScrollBar1.Value).ToString(CultureInfo.InvariantCulture);
        }

        private void SetTreeViewItem()
        {
            //TODO: unfold the trees if needed
            if (!(TreeView1.Items[0] is TreeViewItem productHeader)) return;
            if (!(productHeader.Items[0] is TreeViewItem coreProductHeader)) return;
            if (coreProductHeader.Items[0] is TreeViewItem core1) core1.IsSelected = true;
        }

        private void ShowTreeInTextBlockButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewTextBlock.Text = AllTreeListItems(TreeView1.Items);
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressBar1.Value = Slider1.Value;
            var reverseValue = Slider1.Maximum - (Slider1.Value - Slider1.Minimum);
            ScrollBar1.Value = reverseValue;
            SliderTextBlock.Text = Convert.ToInt32(Slider1.Value).ToString(CultureInfo.InvariantCulture);
        }

        private void TextBlockDrop(object sender, DragEventArgs e)
        {
            if (!(sender is TextBlock textBlock) || !e.Data.GetDataPresent(DataFormats.StringFormat)) return;
            var dataString = (string) e.Data.GetData(DataFormats.StringFormat);
            var converter = new BrushConverter();
            if (!converter.IsValid(dataString))
            {
                textBlock.Text = "Could not convert '" + dataString + "' into a color";
                dataString = "LightSteelBlue";
            }
            else
            {
                textBlock.Text = "Color is " + dataString;
            }
            textBlock.Background = (Brush) converter.ConvertFromString(dataString);
        }

        private void ThreeStateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Checked ThreeStateCheckBox";
        }

        private void ThreeStateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Unchecked ThreeStateCheckBox";
        }

        private void DataGridHeaderCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            DataGrid1.HeadersVisibility = DataGridHeadersVisibility.All;
        }

        private void DataGridHeaderCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            DataGrid1.HeadersVisibility = DataGridHeadersVisibility.None;
        }

        private string CurrentCellFor(DataGrid grid)
        {
            if (grid.SelectedCells.Count == 0) return null;
            var cellInfo = DataGrid1.SelectedCells[0];
            return (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBox)?.Text;
        }

        private void DataGrid1_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GridTextbox.Text = CurrentCellFor(DataGrid1) ?? "Nothing selected.";
        }

    }
}