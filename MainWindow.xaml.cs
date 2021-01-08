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
        private void FsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ActiveNode = e.NewValue as GameFilesystemNode;
        }
        #endregion
    }


    #region Struct data UI presentations
    public class StructBinEnumDataUIPresentation
    {
        private StructBinEnumData _src = null;
        private SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.GetString(_src.NameStrId);
        public uint Unknown => _src.Unknown;

        public StructBinEnumDataUIPresentation(StructBinEnumData src, SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }
    }

    public class StructBinStructDataUIPresentation
    {
        private StructBinStructData _src = null;
        private SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.GetString(_src.NameStrId);
        public int FirstFieldId => _src.FirstFieldId;
        public int FieldCount => _src.FieldCount;
        public string ProgramText => GetCLikeDefinition();

        public StructBinStructDataUIPresentation(StructBinStructData src, SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }

        public string GetCLikeDefinition()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("{ \n");
            for (var fieldId = _src.FirstFieldId; fieldId < _src.FirstFieldId + _src.FieldCount; fieldId++)
            {
                var field = _sbin.FieldDatas[fieldId];

                // Padding
                stringBuilder.Append($"  ");

                // Type
                stringBuilder.Append(field.TypeName?.ToLower() ?? $"unk_0x{field.Type:X}_t");
                if (field.Type == 0x10)
                {
                    var childStructRef = _sbin.StructDatas[field.ChildKind];
                    stringBuilder.Append($"<{_sbin.GetString(childStructRef.NameStrId)}>");
                }
                else if (field.Type == 0x11)
                {
                    var childFieldRef = _sbin.FieldDatas[field.ChildKind];
                    if (childFieldRef.Type == 0x10)
                    {
                        stringBuilder.Append($"<{_sbin.GetString(_sbin.StructDatas[childFieldRef.ChildKind].NameStrId)}>");
                    }
                    else
                    {
                        stringBuilder.Append($"<0x{field.ChildKind:X}>");
                    }
                }
                else if (field.Type == 0x12)
                {
                    var childFieldRef = _sbin.EnumDatas[field.ChildKind];
                    stringBuilder.Append($"<{_sbin.GetString(childFieldRef.NameStrId)}>");
                }

                // Space
                stringBuilder.Append($" ");

                // Field name
                stringBuilder.Append($"{_sbin.GetString(field.NameStrId)}");

                // Semicolon and a new line
                stringBuilder.Append(";");
                if (_src.FirstFieldId + _src.FieldCount - 1 != fieldId)
                {
                    stringBuilder.Append("\n");
                }
            }

            stringBuilder.Append("\n}");
            return stringBuilder.ToString();
        }

    }

    public class StructBinFieldDataUIPresentation
    {
        private StructBinFieldData _src = null;
        private SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.Strings[_src.NameStrId].Ascii;
        public string Type => _src.TypeName ?? $"unk_0x{_src.Type:X}_t";
        public string SizeDesc => GetSizeDesc(_src.Size);
        public int Offset  => _src.Offset;
        public int ChildKind => _src.ChildKind;

        public StructBinFieldDataUIPresentation(StructBinFieldData src, SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }

        private string GetSizeDesc(int size)
            => size switch
            {
                1 => "BYTE",
                2 => "WORD",
                4 => "DWORD",
                8 => "QWORD",
                _ => $"?({size})?"
            };
    }
    #endregion


    #region Converters
    public class StructBinEnumConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<StructBinEnumDataUIPresentation>(
                (value as SemanticStructBin)?.EnumDatas.Select(ed => new StructBinEnumDataUIPresentation(ed, sbin))
                ?? throw new ArgumentException("Passed value should be StructEnums", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StructBinStructConverter
    : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<StructBinStructDataUIPresentation>(
                (value as SemanticStructBin)?.StructDatas.Select(sd => new StructBinStructDataUIPresentation(sd, sbin))
                ?? throw new ArgumentException("Passed value should be StructDatas", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StructBinFieldConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<StructBinFieldDataUIPresentation>(
                (value as SemanticStructBin)?.FieldDatas.Select(fd => new StructBinFieldDataUIPresentation(fd, sbin))
                ?? throw new ArgumentException("Passed value should be FieldDatas", nameof(value)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StructBinStringConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<StructBinString>(sbin.Strings);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
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
            => throw new NotImplementedException();
    }

    public class StructBinToTextConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
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
                stringBuilder.AppendLine($" - Data start = {entry.Start + 12}");
                stringBuilder.AppendLine(entry.DataLength == entry.RealDataLength
                    ? $" - Length = {entry.DataLength}"
                    : $" - Length = {entry.DataLength}, aligned = {entry.RealDataLength}");
                stringBuilder.AppendLine($" - Hash = 0x{entry.Hash:x4}");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    #endregion
}
