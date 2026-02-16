using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using EmployeesOverlap.Core;
using System.IO;

namespace EmployeesOverlap.Wpf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    private async void PickFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Multiselect = false
        };

        if (dlg.ShowDialog() != true)
            return;

        try
        {
            _vm.SelectedFile = dlg.FileName;

            var apiBase = new Uri("http://localhost:5000/");

            using var http = new HttpClient { BaseAddress = apiBase };

            using var form = new MultipartFormDataContent();
            using var fs = File.OpenRead(dlg.FileName);
            using var fileContent = new StreamContent(fs);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            form.Add(fileContent, "file", Path.GetFileName(dlg.FileName));

            var resp = await http.PostAsync("api/overlap/analyze", form);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException(body);

            var best = JsonSerializer.Deserialize<PairResult>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (best == null)
                throw new InvalidOperationException("Empty response from API.");

            _vm.BestPairText = (best.Employee1 == 0 && best.Employee2 == 0)
                ? "(no overlapping pairs found)"
                : $"{best.Employee1}, {best.Employee2}";

            _vm.TotalDays = best.TotalDays;

            _vm.Rows.Clear();
            foreach (var row in best.ProjectRows)
                _vm.Rows.Add(row);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private string _selectedFile = "";
        private string _bestPairText = "(pick a file)";
        private int _totalDays;

        public string SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value; OnPropertyChanged(); }
        }

        public string BestPairText
        {
            get => _bestPairText;
            set { _bestPairText = value; OnPropertyChanged(); }
        }

        public int TotalDays
        {
            get => _totalDays;
            set { _totalDays = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProjectOverlapRow> Rows { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
