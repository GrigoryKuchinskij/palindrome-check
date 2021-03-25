using PalindromeCheckClient.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalindromeCheckClient.ViewModels
{
    class MainWindowViewModel : BindableBase //BaseViewModel
    {
        readonly MainWindowModel _model = new MainWindowModel();
        public MainWindowViewModel()
        {
            _model.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            OpenCommand = new DelegateCommand(() =>
            {
                _model.ShowOpenDialogForFolderPath();
                
                if (_model.FillDataGridWithFolderPath() == -1)
                {
                    _model.URI = "http ://127.0.0.1:8080/";
                }
            });
            CheckCommand = new DelegateCommand(() => 
            {
                if (URI.Trim() != "" && (URI.Trim().IndexOf("http://") != -1 || URI.Trim().IndexOf("https://") != -1))
                    _model.CheckPalindrome();
            });
        }
        
        public string URI { get => _model.URI; set => _model.URI = value; }

        public string FolderPath { get => _model.FolderPath; set => _model.FolderPath = value; }

        public DelegateCommand OpenCommand { get; }
        public DelegateCommand CheckCommand { get; }
        public ReadOnlyObservableCollection<FileDataItem> DGFilesItems => _model.PublicCollectionForDG;
        public ReadOnlyObservableCollection<SimilarityTPalItem> DGSimTPalItems => _model.SimTPalPublicCollection;
    }    
}
