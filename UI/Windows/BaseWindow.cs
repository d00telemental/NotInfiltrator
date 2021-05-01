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

namespace NotInfiltrator.UI.Windows
{
    public abstract class BaseWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        protected TextBlock StatusTextBlock { get; set; }
        protected string BaseWindowTitle { get; set; } = "ME:I Research Tool";

        protected void ExecuteOnUI(Action action)
            => Application.Current.Dispatcher.Invoke(action);
        protected async void ExecuteOnUI(Action action, string status, int delayMs = 500)
        {
            UpdateStatusText(status);
            Dispatcher.Invoke(action, DispatcherPriority.ContextIdle);
            await Task.Delay(delayMs);
            //ResetStatusText();
        }

        protected void UpdateStatusText(string text)
            => ExecuteOnUI(() => { StatusTextBlock.Text = text; });
        protected void ResetStatusText()
            => ExecuteOnUI(() => { StatusTextBlock.Text = null; });

        protected void UpdateWindowTitle(string title)
            => ExecuteOnUI(() => { Title = $"{title} - {BaseWindowTitle}"; });
        protected void ResetWindowTitle()
            => ExecuteOnUI(() => { Title = BaseWindowTitle; });

    }
}
