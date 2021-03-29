using PalindromeCheckClient.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PalindromeCheckClient.ViewModels
{
    class MainWindowViewModel : BindableBase
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
                    _model.FolderPath = "ошибка чтения файлов";
                    Thread.Sleep(500);
                    _model.FolderPath = "";
                }
            });
            CheckCommand = new DelegateCommand(() => 
            {
                if (URI.Trim() != "" && (URI.Trim().IndexOf("http://") != -1 || URI.Trim().IndexOf("https://") != -1))
                    _model.CheckPalindrome();
                else
                {
                    _model.URI = "неверный адрес";
                    Thread.Sleep(500);
                    _model.URI = "http ://127.0.0.1:8080/"; 
                }
            });
        }
        
        public string URI { get => _model.URI; set => _model.URI = value; }
        public bool FolderPathBtnIsEnabled { get => _model.FolderPathBtnIsEnabled; set => _model.FolderPathBtnIsEnabled = value; }
        public bool CheckPalindromeBtnIsEnabled { get => _model.CheckPalindromeBtnIsEnabled; set => _model.CheckPalindromeBtnIsEnabled = value; }
        public string FolderPath { get => _model.FolderPath; set => _model.FolderPath = value; }

        public DelegateCommand OpenCommand { get; }
        public DelegateCommand CheckCommand { get; }
        public ReadOnlyObservableCollection<FileDataItem> DGFilesItems => _model.PublicCollectionForDG;
        public ReadOnlyObservableCollection<SimilarityTPalItem> DGSimTPalItems => _model.SimTPalPublicCollection;
    }    
}
