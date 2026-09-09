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
        public MainWindow()
        {
            InitializeComponent();
            LoadProcesses();
        }
        private void LoadProcesses()
        {
            var processes = Process.GetProcesses()
                .Select(p => new ProcessRow
                {
                    Name = p.ProcessName,
                    Id = p.Id,
                    Memory = $"{p.WorkingSet64 / 1024d / 1024d:N1} MB"
                })
                .OrderBy(p => p.Name)
                .ToList();

            ProcessesGrid.ItemsSource = processes;
        }
    }
}