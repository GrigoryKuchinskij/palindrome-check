using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalindromeCheckClient.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        private string _URI;
        //private List<string> _isPali = new List<string>();
        public string URI
        { 
            get => _URI;
            set 
            {
                _URI = value;
                OnPropertyChanged(nameof(URI));
            }
        }

        //public List<string> IsPali
        //{
        //    get => _isPali;
        //    set
        //    {
        //        _isPali = value;
        //        OnPropertyChanged(nameof(IsPali));
        //    }
        //}
    }
}
