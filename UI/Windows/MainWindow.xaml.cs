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

using NotInfiltrator.Serialization;
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
}
