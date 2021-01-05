using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotInfiltrator
{
    public partial class MainWindow : Window
    {
        private GameFilesystem Fs = new GameFilesystem(@"D:\Projects\NotInfiltrator\_game\com.ea.games.meinfiltrator_gamepad\published\");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UI(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        private void UpdateStatus(string text)
        {
            UI(() => { StatusBar_Status.Text = text; });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                UpdateStatus("Loading SBINs");
                Fs.LoadAllStructBins();

                UpdateStatus("Building filesystem tree");
                Fs.BuildFileTree();

                UpdateStatus("Updating UI");
                UI(() => { FsTreeView.Items.Add(Fs.Root); });

                UpdateStatus("");
                return Task.CompletedTask;
            });
        }

        private void FsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as GameFilesystemNode;
            if (node == null) { return; }

            var nodeContent = node.Content as StructBin;
            if (nodeContent == null) { return; }

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{node.Name}");
            stringBuilder.AppendLine();

            foreach (var entry in nodeContent.Entries)
            {
                stringBuilder.AppendLine($"Entry '{entry.Label}'");
                stringBuilder.AppendLine($" - Start = {entry.Start}, end = {entry.End}");
                stringBuilder.AppendLine(entry.DataLength == entry.RealDataLength
                    ? $" - Length = {entry.DataLength}"
                    : $" - Length = {entry.DataLength}, real = {entry.RealDataLength}");
                stringBuilder.AppendLine($" - Hash = 0x{entry.Hash:x4}");
                stringBuilder.AppendLine();
            }

            RawSBin_TextBox.Text = stringBuilder.ToString();
        }
    }
}
