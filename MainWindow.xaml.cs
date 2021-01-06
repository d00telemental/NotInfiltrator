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

            //StructSBin_ChardataListBox.ItemsSource
        }
    }

    public class StructBinString
    {
        public int Id { get; set; } = 0;
        public Int32 Offset { get; set; } = 0;
        public Int32 Length { get; set; } = 0;
        public string Ascii { get; set; } = null;
    }

    public class StructBinStringParser
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Gets a StructBin
            // Returns an ObservableCollection<StructBinString>

            var sbin = value as StructBin ?? throw new ArgumentException("Passed value should be StructBin", nameof(value));

            var chdrSection = sbin.FindSection("CHDR");
            var cdatSection = sbin.FindSection("CDAT");

            var strings = new List<StructBinString>();
            var chdrSectionStream = new MemoryStream(chdrSection.Data);
            while (chdrSectionStream.Position < chdrSectionStream.Length)
            {
                var offset = chdrSectionStream.ReadSigned32Little();
                var length = chdrSectionStream.ReadSigned32Little();

                strings.Add(new StructBinString {
                    Id = strings.Count(),
                    Offset = offset,
                    Length = length,
                    Ascii = Encoding.ASCII.GetString(cdatSection.Data.Skip(offset).Take(length).ToArray())
                });
            }

            return new ObservableCollection<StructBinString>(strings);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SBinToTextConverter
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
}
