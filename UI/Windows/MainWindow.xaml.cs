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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Internal fields
        private GameFilesystem Filesystem = null;

        private readonly string BaseWindowTitle = "ME:Infiltrator Data Explorer";
        #endregion

        #region Non-bindable properties

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

        #region Common UI procedures
        private static void ExecuteOnUIThread(Action action)
            => Application.Current.Dispatcher.Invoke(action);

        private void UpdateStatus(string text)
            => ExecuteOnUIThread(() => { StatusBar_Status.Text = text; });

        private void ResetStatus()
            => UpdateStatus(null);

        private async void ExecuteOnUIWithStatus(Action action, string status, int delayMs = 500)
        {
            UpdateStatus(status);
            Dispatcher.Invoke(action, DispatcherPriority.ContextIdle);
            await Task.Delay(delayMs);
            ResetStatus();
        }

        private void UpdateWindowTitle(string title)
            => ExecuteOnUIThread(() => { Title = $"{title} - {BaseWindowTitle}"; });

        private void ResetWindowTitle()
            => ExecuteOnUIThread(() => { Title = BaseWindowTitle; });
        #endregion

        #region Window procedures
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                ResetWindowTitle();
                LoadFilesystem(@"D:\Projects\NotInfiltrator\_game\com.ea.games.meinfiltrator_gamepad\published\");
                return Task.CompletedTask;
            });
        }
        private void FsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Task.Run(() => {
                var selection = e.NewValue as GameFilesystemNode;
                ExecuteOnUIWithStatus(() => {
                    ActiveNode = selection;
                    UpdateWindowTitle(selection.Name);
                }, $"Updating user interface for {selection.Name}");
                GC.Collect();  // saves up to 100% of memory after some time of switching between GameFilesystemNodes.
            });
        }
        #endregion

        #region Window logic
        private void LoadFilesystem(string rootPath)
        {
            ExecuteOnUIWithStatus(() => {
                Filesystem = new GameFilesystem(rootPath);
            }, "Loading filesystem");

            ExecuteOnUIWithStatus(() => {
                ExecuteOnUIThread(() => { FsTreeView.Items.Add(Filesystem.RootNode); });
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
                    StructObjectField { MetaData: { Type: FieldType.Int8 } } => element.FindResource("IntegerValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt8 } } => element.FindResource("IntegerValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Int16 } } => element.FindResource("IntegerValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt16 } } => element.FindResource("IntegerValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Int32 } } => element.FindResource("IntegerValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.UInt32 } } => element.FindResource("IntegerValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Boolean } } => element.FindResource("BooleanValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Float } } => element.FindResource("FloatValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Double } } => element.FindResource("FloatValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.String } } => element.FindResource("StringValue") as DataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Reference } } => element.FindResource("ReferenceFieldValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.InlineStruct } } => element.FindResource("InlineStructFieldValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Array } } => element.FindResource("ArrayValue") as HierarchicalDataTemplate,
                    StructObjectField { MetaData: { Type: FieldType.Enum } } => element.FindResource("EnumValue") as DataTemplate,
                    ArrayObject => element.FindResource("ArrayValue") as HierarchicalDataTemplate,

                    ReferenceValue rv when rv.Value is StructObject => element.FindResource("StructObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue rv when rv.Value is UnstructuredObject => element.FindResource("UnstructuredObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue rv when rv.Value is ArrayObject => element.FindResource("ArrayObjectValue") as HierarchicalDataTemplate,
                    ReferenceValue { IsNil: true } => element.FindResource("NilReferenceValue") as DataTemplate,

                    StructObject => element.FindResource("StructObjectValue") as HierarchicalDataTemplate,

                    UnstructuredObjectEntry { Type: FieldType.Int32 } => element.FindResource("UnstructuredObjectIntegerValue") as DataTemplate,
                    UnstructuredObjectEntry => element.FindResource("UnstructuredObjectEntryValue") as HierarchicalDataTemplate,

                    Int32Value => element.FindResource("IntegerValue") as DataTemplate,
                    InlineStructValue => element.FindResource("InlineStructValue") as HierarchicalDataTemplate,

                    _ => element.FindResource("OtherValue") as DataTemplate
                };
            }
            return null;
        }
    }
}
