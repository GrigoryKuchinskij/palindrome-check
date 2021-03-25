using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Web;
using System.Threading;
using System.ComponentModel;
using PalindromeCheckClient.ViewModels;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace PalindromeCheckClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            //URITbx.Text = _defURI;
            //this.DataContext = mWinVM;
            //mWinVM.URI = URIstr;            
        }
    }
}
