using Pulse.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Pulse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private List<ProcessRow> _allProcesses = [];
        private readonly Dictionary<int, (TimeSpan CpuTime, DateTime SimpleTime)> _cpuSamples = [];
        private readonly DispatcherTimer _refreshTimer = new()
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        public MainWindow()
        {
            InitializeComponent();
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
            //LoadProcesses();
        }
        private void LoadProcesses()
        {
            _allProcesses = Process.GetProcesses()
                .Select(CreateProcessRow)
                .OrderBy(p => p.Name)
                .ToList();

            ApplyFilter();
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

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            LoadProcesses();
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
        private double? GetCpuPercent(Process process)
        {
            TimeSpan _cpuTime;

            try
            {
                _cpuTime = process.TotalProcessorTime;
            }
            catch
            {
                return null;
            }
            var _now = DateTime.UtcNow;
            

            double _cpuPercent = 0;

            if (_cpuSamples.TryGetValue(process.Id, out var previous))
            {
                var _cpuDelta = _cpuTime - previous.CpuTime;
                var _timeDelta = _now - previous.SimpleTime;

                if (_timeDelta.TotalMilliseconds > 0)
                {
                    _cpuPercent =
                        _cpuDelta.TotalMilliseconds /
                        _timeDelta.TotalMilliseconds /
                        Environment.ProcessorCount *
                        100;
                }
            }

            _cpuSamples[process.Id] = (_cpuTime, _now);

            return _cpuPercent;
        }

        private ProcessRow CreateProcessRow(Process process)
        {

            return new ProcessRow
            {
                Name = process.ProcessName,
                Id = process.Id,
                Memory = $"{process.WorkingSet64 / 1024d / 1024d:N1} MB",
                CpuPercent = GetCpuPercent(process)
            };
        }
    }
}