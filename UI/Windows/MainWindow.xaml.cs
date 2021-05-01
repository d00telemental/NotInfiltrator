using System;
using System.Collections;
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
using System.Windows.Threading;

using NotInfiltrator.Serialization;
using NotInfiltrator.Serialization.Data;
using NotInfiltrator.Serialization.Monkey;
using NotInfiltrator.Utilities;

namespace NotInfiltrator.UI.Windows
{
    public partial class MainWindow : BaseWindow, INotifyPropertyChanged
    {
        private GameFilesystem _filesystem = null;

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

        private EnumData _activeEnumData = null;
        public EnumData ActiveEnumData
        {
            get { return _activeEnumData; }
            set
            {
                _activeEnumData = value;
                OnPropertyChanged();
            }
        }

        private StructData _activeStructData = null;
        public StructData ActiveStructData
        {
            get { return _activeStructData; }
            set
            {
                _activeStructData = value;
                OnPropertyChanged();
            }
        }

        private ObjectData _activeObjectData = null;
        public ObjectData ActiveObjectData
        {
            get { return _activeObjectData; }
            set
            {
                _activeObjectData = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Window procedures
        public MainWindow()
        {
            InitializeComponent();
            BaseWindowTitle = "ME:Infiltrator Data Research Tool";
            StatusTextBlock = StatusBar_Status;
            DataContext = this;

            Loaded += MainWindow_Loaded;
            FsTreeView.SelectedItemChanged += FsTreeView_SelectedItemChanged;
        }

        ~MainWindow()
        {
            FsTreeView.SelectedItemChanged -= FsTreeView_SelectedItemChanged;
            Loaded -= MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                ResetWindowTitle();
                _loadFilesystem(@"D:\Projects\NotInfiltrator\_game\com.ea.games.meinfiltrator_gamepad\published\");
                return Task.CompletedTask;
            });
        }
        private void FsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Task.Run(() => {
                var selection = e.NewValue as GameFilesystemNode;
                ExecuteOnUI(() => {
                    ActiveNode = selection;
                    UpdateWindowTitle(selection.Name);
                }, $"Updating UI for {selection.Name}");
                GC.Collect();  // saves up to 100% of memory after some time of switching between GameFilesystemNodes.
            });
        }
        #endregion

        #region Window logic
        private void _loadFilesystem(string rootPath)
        {
            ExecuteOnUI(() => {
                _filesystem = new GameFilesystem(rootPath);
            }, "Loading filesystem");

            ExecuteOnUI(() => {
                ExecuteOnUI(() => { FsTreeView.Items.Add(_filesystem.RootNode); });
            }, "Updating user interface");
        }
        #endregion
    }

    public class MonkeyValueDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element != null && item != null)
            {
                return item switch
                {
                    StructObjectField { MetaData: { Type: FieldType.Int8 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt8 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Int16 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt16 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Int32 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt32 } } => element.FindResource("IntegerFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Boolean } } => element.FindResource("BooleanFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Float } } => element.FindResource("FloatFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Double } } => element.FindResource("FloatFieldValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.String } } => element.FindResource("StringValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Reference } } => element.FindResource("ReferenceFieldValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.InlineStruct } } => element.FindResource("InlineStructFieldValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Array } } => element.FindResource("ArrayValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Enum } } => element.FindResource("EnumValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Symbol } } => element.FindResource("SymbolValue") as DataTemplate,
                    ArrayObject => element.FindResource("ArrayValue") as HierarchicalDataTemplate,

                    ReferenceValue rv when rv.Value is StructObject => element.FindResource("StructObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue rv when rv.Value is UnstructuredObject => element.FindResource("UnstructuredObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue rv when rv.Value is ArrayObject => element.FindResource("ArrayObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue { IsNil: true } => element.FindResource("NilReferenceValue") as DataTemplate,

                    StructObject => element.FindResource("StructObjectValue") as HierarchicalDataTemplate,

                    UnstructuredObjectEntry { Type: FieldType.Int8 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt8 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Int16 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt16 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Int32 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt32 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Int64 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.UInt64 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Boolean } => element.FindResource("UnstructuredObjectBooleanValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Float } => element.FindResource("UnstructuredObjectFloatValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.Double } => element.FindResource("UnstructuredObjectFloatValue") as DataTemplate,
                    UnstructuredObjectEntry { Type: FieldType.String } => element.FindResource("UnstructuredObjectStringValue") as DataTemplate,

                    UnstructuredObjectEntry => element.FindResource("UnstructuredObjectEntryValue") as HierarchicalDataTemplate,

                    Int8Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt8Value => element.FindResource("IntegerValue") as DataTemplate,
                    Int16Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt16Value => element.FindResource("IntegerValue") as DataTemplate,
                    Int32Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt32Value => element.FindResource("IntegerValue") as DataTemplate,
                    Int64Value => element.FindResource("IntegerValue") as DataTemplate,
                    UInt64Value => element.FindResource("IntegerValue") as DataTemplate,
                    BooleanValue => element.FindResource("BooleanValue") as DataTemplate,
                    FloatValue => element.FindResource("FloatValue") as DataTemplate,
                    DoubleValue => element.FindResource("FloatValue") as DataTemplate,
                    //StringValue => element.FindResource("StringValue") as DataTemplate,
                    InlineStructValue => element.FindResource("InlineStructValue") as HierarchicalDataTemplate,

                    _ => element.FindResource("OtherValue") as DataTemplate
                };
            }
            return null;
        }
    }
}
