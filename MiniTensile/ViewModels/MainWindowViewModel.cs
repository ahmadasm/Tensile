using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MiniTensile.Models;
namespace MiniTensile.ViewModels
{
    public class MainWindowViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        static string hostName = "127.0.0.1";
        static int portNumber = 8096;
        IParser<IControlData> _parser;
        AsyncClient asyncClient;
        static int counter = 0;
        public MainWindowViewModel()
        {
            _parser = new ParserApsTensileV1();
            asyncClient = new AsyncClient(hostName, portNumber);
            asyncClient.MessageReceived += AsyncClient_MessageReceived;
            asyncClient.Connect();
            
        }

        private double loadData;
        public double LoadCell
        {
            get => loadData;
            set
            {
                loadData = value;
                OnPropertyChanged("LoadCell");
            }
        }
        public ICommand StartCommand => new RelayCommand(() => Start());
        public ICommand StopCommand => new RelayCommand(() => Stop());
        public ICommand PauseCommand => new RelayCommand(() => Pause());
        private void Start()
        {
            if (asyncClient.IsConnected)
            {
                asyncClient.Send("start\r");
            }
        }
        private void Stop()
        {
            if (asyncClient.IsConnected)
            {
                asyncClient.Send("stop\r");
            }
        }
        private void Pause()
        {
            if (asyncClient.IsConnected)
            {
                asyncClient.Send("pause\r");
            }
        }
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void AsyncClient_MessageReceived(object sender, string message)
        {
            IControlData data = _parser.Pars(message);
            if (data != null)
            {
                counter++;
                this.LoadCell = (double)data.Value*10;
                Console.WriteLine(counter);
            }
        }
    }
    class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action _action;

        public RelayCommand(Action action)
        {
            _action = action;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
