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

namespace NotInfiltrator.UI.Windows
{
    /// <summary>
    /// Interaction logic for StructBinWindow.xaml
    /// </summary>
    public partial class StructBinWindow : BaseWindow
    {
        public StructBinWindow()
        {
            InitializeComponent();
            //StatusTextBlock = StatusBar_Status;
            DataContext = this;

            UpdateWindowTitle("StructBin Tool");
        }

        public StructBinWindow(GameFilesystemNode contentNode)
            : base()
        {
            if (contentNode is not null && contentNode.Content is not StructBin sbin)
            {
                throw new ArgumentException();
            }

            UpdateWindowTitle($"{contentNode.Name} - StructBin Tool");
        }
    }
}
