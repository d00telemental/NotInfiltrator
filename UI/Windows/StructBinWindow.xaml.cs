using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

using NotInfiltrator.Serialization;
using NotInfiltrator.Serialization.Monkey;
using NotInfiltrator.Serialization.Monkey.Data;

namespace NotInfiltrator.UI.Windows
{
    /// <summary>
    /// Interaction logic for StructBinWindow.xaml
    /// </summary>
    public partial class StructBinWindow : BaseWindow
    {
        private StructBin _activeStructBin = null;
        public StructBin ActiveStructBin
        {
            get { return _activeStructBin; }
            set
            {
                _activeStructBin = value;
                OnPropertyChanged();
            }
        }

        private StringData _activeStringData = null;
        public StringData ActiveStringData
        {
            get { return _activeStringData; }
            set
            {
                _activeStringData = value;
                OnPropertyChanged();
            }
        }


        public StructBinWindow()
        {
            InitializeComponent();
            StatusTextBlock = StatusBar_Status; 
            DataContext = this;

            BaseWindowTitle = $"SBIN tool - {BaseWindowTitle}";
            ResetWindowTitle();

            ResetStatusText();
        }

        public StructBinWindow(GameFilesystemNode contentNode)
            : this()
        {
            if (contentNode is not null && contentNode.Content is not StructBin sbin)
            {
                throw new ArgumentException();
            }

            ActiveStructBin = contentNode.Content as StructBin;
            UpdateWindowTitle($"{contentNode.Name}");
        }
    }
}
