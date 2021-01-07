using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotInfiltrator
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Non-bindable properties
        private GameFilesystem Fs = new GameFilesystem(@"D:\Projects\NotInfiltrator\_game\com.ea.games.meinfiltrator_gamepad\published\");
        #endregion

        #region Bindable Properties
        private GameFilesystemNode _activeNode = null;
        public GameFilesystemNode ActiveNode
        {
            get { return _activeNode; }
            set
            {
                _activeNode = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Common UI procedures
        private void ExecuteOnUIThread(Action action)
            => Application.Current.Dispatcher.Invoke(action);

        private void UpdateStatus(string text)
            => ExecuteOnUIThread(() => { StatusBar_Status.Text = text; });
        #endregion

        #region Window logic
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                UpdateStatus("Loading SBINs");
                Fs.LoadAllStructBins();

                UpdateStatus("Building filesystem tree");
                Fs.BuildFileTree();

                UpdateStatus("Updating UI");
                ExecuteOnUIThread(() => { FsTreeView.Items.Add(Fs.Root); });

                UpdateStatus("");
                return Task.CompletedTask;
            });
        }
        #endregion

        private void FsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ActiveNode = e.NewValue as GameFilesystemNode;
        }
    }

    public class StructBinFieldDataUIPresentation
    {
        public string Name { get; set; } = null;
        public string Type { get; set; } = null;
        public int Offset { get; set; } = 0;
        public int Unknown { get; set; } = 0;

        public StructBinFieldDataUIPresentation(StructBinFieldData src, SemanticStructBin sbin)
        {
            Name = sbin.Strings[src.NameStrId].Ascii;
            Type = $"0x{src.Type:X} (n/a)";
            Offset = src.Offset;
            Unknown = src.Unknown;
        }
    }

    #region Converters
    public class StructBinFieldConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var sbin = mainWindow.ActiveNode?.Content as SemanticStructBin ?? throw new Exception("Failed to get current SBIN because it was null");

            return new ObservableCollection<StructBinFieldDataUIPresentation>(
                (value as SemanticStructBin)?.FieldDatas.Select(fd => new StructBinFieldDataUIPresentation(fd, sbin))
                ?? throw new ArgumentException("Passed value should be FieldDatas", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StructBinStringParser
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new ObservableCollection<StructBinString>(
                (value as SemanticStructBin)?.Strings
                ?? throw new ArgumentException("Passed value should be StructBin", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToHexConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var num = long.Parse($"{value}");
            return $"0x{value:X}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StructBinToTextConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as StructBin;
            if (sbin == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(sbin.FileName);
            stringBuilder.AppendLine();

            foreach (var entry in sbin.Sections)
            {
                stringBuilder.AppendLine($"Section '{entry.Label}'");
                stringBuilder.AppendLine($" - Start = {entry.Start}, end = {entry.End}");
                stringBuilder.AppendLine(entry.DataLength == entry.RealDataLength
                    ? $" - Length = {entry.DataLength}"
                    : $" - Length = {entry.DataLength}, real = {entry.RealDataLength}");
                stringBuilder.AppendLine($" - Hash = 0x{entry.Hash:x4}");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
