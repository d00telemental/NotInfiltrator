using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using NotInfiltrator.Serialization;

namespace NotInfiltrator.UI.Windows
{
    /// <summary>
    /// Interaction logic for NavigatorWindow.xaml
    /// </summary>
    public partial class NavigatorWindow : BaseWindow
    {
        #region Dialog-style window
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;
        private const int WS_SYSMENU = 0x80000;

        private void NavigatorWindow_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            var extStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(extStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }
        #endregion

        private GameFilesystem _filesystem = null;

        private void _loadFilesystem(string rootPath)
        {
            ExecuteOnUI(() => {
                _filesystem = new GameFilesystem(rootPath);
            }, "Loading filesystem...");

            ExecuteOnUI(() => {
                ObservableRootNode = new () { _filesystem.RootNode };
            }, "Updating user interface...");
        }

        private void _handleTreeViewSelection(GameFilesystemNode node)
        {
            Action action = node.Content switch
            {
                StructBin sbin => () => {
                    Debug.WriteLine($"Opening SBIN tool for {node.GetPath()}...");
                    var sbinWindow = new StructBinWindow(node);  // TODO: add some window tracking
                    sbinWindow.Show();
                },
                MediaContainer mc => () =>
                {
                    Debug.WriteLine($"Opening M3G tool for {node.GetPath()}...");
                    var m3gWindow = new MediaContainerWindow(node);
                    m3gWindow.Show();
                },
                _ => () => Debug.WriteLine($"Can't handle an attempt to open {node.GetPath()}")
            };

            action.Invoke();
        }

        #region Notifying properties
        private ObservableCollection<GameFilesystemNode> _filteredRootNode = null;
        public ObservableCollection<GameFilesystemNode> FilteredRootNode
        {
            get => _filteredRootNode;
            set
            {
                _filteredRootNode = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GameFilesystemNode> _observableRootNode = null;
        public ObservableCollection<GameFilesystemNode> ObservableRootNode
        {
            get => _observableRootNode;
            set
            {
                FilteredRootNode = value;
                _observableRootNode = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public NavigatorWindow()
        {
            InitializeComponent();
            StatusTextBlock = StatusBar_Status;
            DataContext = this;

            ResetStatusText();
            ResetWindowTitle();

            SourceInitialized += NavigatorWindow_SourceInitialized;
            Loaded += NavigatorWindow_Loaded;
        }
        ~NavigatorWindow()
        {
            Loaded -= NavigatorWindow_Loaded;
            SourceInitialized -= NavigatorWindow_SourceInitialized;
        }

        private void NavigatorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Mount the database / filesystem

            Task.Run(() =>
            {
                ResetWindowTitle();
                _loadFilesystem(@"D:\Projects\NotInfiltrator\_game\com.ea.games.meinfiltrator_gamepad\published\");
                UpdateStatusText("Database loaded");

                return Task.CompletedTask;
            }).ContinueWith((Task t) =>
            {
                ExecuteOnUI(() =>
                {
                    var nodeToSelectInDebug = _filesystem.FindNode(@"models\env_snow_planet.m3g");
                    Debug.WriteLine($"Selecting {nodeToSelectInDebug} because debug");
                    _handleTreeViewSelection(nodeToSelectInDebug);
                });
                return Task.CompletedTask;
            });
        }

        private void DbTreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        private void DbTreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TreeViewItem tvItem || DbTreeView.SelectedItem is not GameFilesystemNode gfsNode)
            {
                throw new Exception();
            }

            // Do not handle events for parent nodes.
            if (tvItem.Items.Count != 0)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Return:
                    _handleTreeViewSelection(gfsNode);
                    return;
            }
        }
        private void DbViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TreeViewItem tvItem || DbTreeView.SelectedItem is not GameFilesystemNode gfsNode)
            {
                throw new Exception();
            }

            // Do not handle events for parent nodes.
            if (tvItem.Items.Count == 0)
            {
                _handleTreeViewSelection(gfsNode);
            }
        }
    }
}
