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
using NotInfiltrator.Serialization.Monkey;
using NotInfiltrator.Serialization.Monkey.Data;
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
}
