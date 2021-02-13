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
    public class StructBinSectionDataUIPresentation
    {
        private StructBinSection _src = null;
        private SemanticStructBin _sbin = null;

        public string Label => _src.Label;

        public long Start => _src.Start;

        public int Length => _src.DataLength;

        public int AlignedLength => _src.RealDataLength;

        public int Hash => _src.Hash;

        public StructBinSectionDataUIPresentation(StructBinSection src, SemanticStructBin sbin)
        {
            _src = src;
            _sbin = sbin;
        }
    }

    public class StructBinEnumDataUIPresentation
    {
        private StructBinEnumData _src = null;
        private SemanticStructBin _sbin = null;

        public int Id => _src.Id;
        public string Name => _sbin.GetString(_src.NameStrId);
        public uint ObjReference => _src.ObjReference;

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

    public class StructBinSectionConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null) { return null; }

            return new ObservableCollection<StructBinSectionDataUIPresentation>(sbin.Sections.Select(s => new StructBinSectionDataUIPresentation(s, sbin)));
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

    public class StructBinToObjHeaderTextConverter
        : IValueConverter
    {
        public class StructBinTempObjectData
        {
            public int Id { get; set; } = 0;
            public byte[] OrigOffset { get; set; }
            public uint Offset { get; set; } = 0;
            public uint Length { get; set; } = 0;

            public StructBinTempObjectData(int id, uint offset, byte[] origOffset)
            {
                Id = id;
                Offset = offset;
                OrigOffset = origOffset;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sbin = value as SemanticStructBin;
            if (sbin == null)
            {
                return string.Empty;
            }

            var stream = sbin.FindSection("OHDR").NewMemoryStream();
            var data = sbin.FindSection("DATA").Data;

            var objs = new List<StructBinTempObjectData>();

            Func<byte[], UInt32> f = (byte[] b) => {
                if (b.Length != 4) throw new NullReferenceException();

                UInt32 res = 0;

                res |= (UInt32)((UInt32)b[0] >> (Int32)0x3);
                res |= (UInt32)((UInt32)b[1] << (Int32)0x5);
                res |= (UInt32)((UInt32)b[2] << (Int32)0xD);
                res |= (UInt32)((UInt32)b[3] << (Int32)0x15);

                return res;
            };

            var stringBuilder = new StringBuilder();


            var counter = 0;
            while (stream.Position < stream.Length)
            {
                var origOffset = stream.ReadBytes(4);
                var offset = f(origOffset);
                if (counter > 0)
                {
                    objs[counter - 1].Length = offset - objs[counter - 1].Offset;
                }
                objs.Add(new StructBinTempObjectData(counter++, offset, origOffset));
            }
            objs[counter - 1].Length = (uint)data.Length - objs[counter - 1].Offset;


            stringBuilder.AppendLine($"Read {counter} indices...");
            stringBuilder.AppendLine("\n");

            foreach (var obj in objs)
            {
                stringBuilder.AppendLine($"Object 0x{obj.Id:X2}  @  0x{obj.Offset:X2}  (len = 0x{obj.Length:X2})  // orig. def. = {BitConverter.ToString(obj.OrigOffset)}");
                stringBuilder.AppendLine();

                stringBuilder.AppendLine(BitConverter.ToString(data.Skip((int)obj.Offset).Take((int)obj.Length).ToArray()).Replace("-", " "));
                stringBuilder.AppendLine("\n");
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    #endregion
}
