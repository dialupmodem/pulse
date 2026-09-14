using Pulse.Models;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace Pulse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private List<ProcessRow> _allProcesses = [];

        public MainWindow()
        {
            InitializeComponent();
            LoadProcesses();
        }
        private void LoadProcesses()
        {
            _allProcesses = Process.GetProcesses()
                .Select(p => new ProcessRow
                {
                    Name = p.ProcessName,
                    Id = p.Id,
                    Memory = $"{p.WorkingSet64 / 1024d / 1024d:N1} MB"
                })
                .OrderBy(p => p.Name)
                .ToList();

            ProcessesGrid.ItemsSource = _allProcesses;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void ApplyFilter()
        {
            var search = SearchBox.Text.Trim();

            ProcessesGrid.ItemsSource = string.IsNullOrWhiteSpace(search)
                ? _allProcesses
                : _allProcesses
                    .Where(p =>
                        p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.Id.ToString().Contains(search))
                    .ToList();
        }

        private void ProcessesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessesGrid.SelectedItem is not ProcessRow processRow)
                return;

            ProcessName.Text = processRow.Name;
            ProcessID.Text = processRow.Id.ToString();
            ProcessMemory.Text = processRow.Memory;
            try
            {
                var _process = Process.GetProcessById(processRow.Id);

                ExecutionPath.Text = TryGet(() => _process.MainModule!.FileName);
                StartTime.Text = TryGet(() => _process.StartTime.ToString("M/d/yyyy h:mm:ss tt"));
                ThreadCount.Text = TryGet(() => _process.Threads.Count.ToString());
                HandleCount.Text = TryGet(() => _process.HandleCount.ToString());
            }

            catch (ArgumentException)
            {
                ClearProcessDetails();
                return;
            }
        }

        private static string TryGet(Func<string> getter)
        {
            try
            {
                return getter();
            }
            catch
            {
                return "Unavailable";
            }
        }
        private void ClearProcessDetails()
        {
            ProcessName.Text = "";
            ProcessID.Text = "";
            ProcessMemory.Text = "";
            ExecutionPath.Text = "";
            StartTime.Text = "";
            ThreadCount.Text = "";
            HandleCount.Text = "";
        }
    }
}