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
using System.Windows.Threading;
using NotInfiltrator.Serialization;
using NotInfiltrator.Serialization.StructBin;
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
        private GameFilesystem Fs = null;

        private readonly string BaseWindowTitle = "ME Infiltrator Data Explorer";
        #endregion

        #region Non-bindable properties
        //public List<StructData> StructDatas => (ActiveNode?.Content as SemanticStructBin)?.StructDatas;
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
                }, $"Parsing {selection.Name}");
            });
        }
        #endregion

        #region Window logic
        public void LoadFilesystem(string rootPath)
        {
            ExecuteOnUIWithStatus(() => {
                Fs = new GameFilesystem(rootPath);
                Fs.Load();
            }, "Loading filesystem");

            ExecuteOnUIWithStatus(() => {
                ExecuteOnUIThread(() => { FsTreeView.Items.Add(Fs.Root); });
            }, "Updating user interface");
        }
        #endregion
    }
}
