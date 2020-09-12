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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Syncfusion.UI.Xaml.Gauges;
using MiniTensile.Models;
using System.Diagnostics;
using System.Threading;
using MiniTensile.ViewModels;

namespace MiniTensile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Mini Tensile";
            this.Height = 600;
            this.Width = 600;
            this.ResizeMode = ResizeMode.CanResize;
            this.DataContext = new MainWindowViewModel();
        }
    }
}
