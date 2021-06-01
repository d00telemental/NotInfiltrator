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
    /// Interaction logic for MediaContainerWindow.xaml
    /// </summary>
    public partial class MediaContainerWindow : BaseWindow
    {
        private MediaContainer _activeMediaContainer = null;
        public MediaContainer ActiveMediaContainer
        {
            get => _activeMediaContainer;
            set
            {
                _activeMediaContainer = value;
                OnPropertyChanged();
            }
        }

        public MediaContainerWindow()
        {
            InitializeComponent();
            StatusTextBlock = StatusBar_Status;
            DataContext = this;

            BaseWindowTitle = $"M3G tool - {BaseWindowTitle}";
            ResetWindowTitle();

            ResetStatusText();
        }

        public MediaContainerWindow(GameFilesystemNode contentNode)
            : this()
        {
            if (contentNode is not null && contentNode.Content is not MediaContainer mediaContainer)
            {
                throw new ArgumentException();
            }

            mediaContainer = contentNode.Content as MediaContainer;
            if (!mediaContainer.Initialized)
            {
                mediaContainer.Initialize();
            }

            ActiveMediaContainer = mediaContainer;
            UpdateWindowTitle($"{contentNode.Name}");
        }
    }
}
