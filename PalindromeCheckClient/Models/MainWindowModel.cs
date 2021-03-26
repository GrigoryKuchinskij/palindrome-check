using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Mvvm;

namespace PalindromeCheckClient.Models
{
    public class MainWindowModel : BindableBase
    {     
        static readonly string defIP = "127.0.0.1";
        static readonly string defPort = "8090";
        private string _URI;
        public string URI { set => _URI = value; get => _URI; }

        private string _FolderPath;
        public string FolderPath { get => _FolderPath; set => _FolderPath = value; }

        private readonly Dispatcher _dispatcher;

        private ObservableCollection<FileDataItem> ObservCollectionForDG = new ObservableCollection<FileDataItem>();
        public ReadOnlyObservableCollection<FileDataItem> PublicCollectionForDG;
        private ObservableCollection<SimilarityTPalItem> SimTPalObservCollection = new ObservableCollection<SimilarityTPalItem>();
        public ReadOnlyObservableCollection<SimilarityTPalItem> SimTPalPublicCollection;

        public MainWindowModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            PublicCollectionForDG = new ReadOnlyObservableCollection<FileDataItem>(ObservCollectionForDG);
            SimTPalPublicCollection = new ReadOnlyObservableCollection<SimilarityTPalItem>(SimTPalObservCollection);
            _URI = "http://"+ defIP + ":"+ defPort + "/";
        }

        public bool FolderPathBtnIsEnabled = true;
        public bool CheckPalindromeBtnIsEnabled = true;

        public void CheckPalindrome()
        {

            FolderPathBtnIsEnabled = false; CheckPalindromeBtnIsEnabled = false;
            RaisePropertyChanged("FolderPathBtnIsEnabled");
            RaisePropertyChanged("CheckPalindromeBtnIsEnabled");
            var thread = new Thread( StartSendFiles )
            { IsBackground = false };
            thread.Start();
            

            //GC.Collect();
        }

        int defWaitServ = 50, waitServ;
        bool limitReached = false;

        private void StartSendFiles()
        {
            int i = 0, avTreads = -1, curTreads = 0; 
            waitServ = defWaitServ;
            _dispatcher.Invoke(new Action(() => {
                SimTPalObservCollection.Clear();
            }));
            foreach (FileDataItem item in ObservCollectionForDG)
            {
                item.Procd = false;                
            }
            while (true)                                                                //проход по всем элементам списка
            {                                 
                if (limitReached && avTreads == -1)                                     //если сервер перегружался и число доступных потоков ещё не задано
                {
                    avTreads = curTreads;                                               //присвоить счетчик текущих потоков => числу доступных потоков
                    //curTreads = 0;
                    //i = 0;
                    //continue;
                    //и начать с начала
                }
                else                                                                    //иначе, увеличить счетчик текущих потоков ...
                    curTreads++;

                if (i >= ObservCollectionForDG.Count)                                   //если последний объект списка
                {
                    int unProcd = 0;
                    foreach (FileDataItem dataItem in ObservCollectionForDG)
                        if (dataItem.Procd == false) { 
                            unProcd++;
                        }
                    if (unProcd > 0)                                                    //если есть необработанные строки
                    {
                        avTreads = Math.Min(unProcd, avTreads);
                        curTreads = 0;
                        i = 0;                                                          //начать с начала
                        continue;
                    }
                    else                                                                //и если все строки обработаны
                    {
                        break;                                                          //закончить
                    };
                };

                if (ObservCollectionForDG[i].Procd == true)                             //если текущая строка обработана
                {
                    i++;
                    continue;                                                           //пропустить
                };

                var request = (HttpWebRequest)HttpWebRequest.Create(_URI);
                request.Method = "POST";
                string postData = "IsPalindrome=&word=" + ObservCollectionForDG[i].Text + "&index=" + i;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "text/html";
                request.ContentLength = byteArray.Length;
                try
                {
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }
                catch
                {
                    MessageBox.Show("Невозможно соединиться с удаленным сервером");     //System.Net.WebException: "Невозможно соединиться с удаленным сервером";
                    break;
                };

                var thread =
                    new Thread(new ThreadStart(() =>
                    {
                        ReceiveHtml(request, out string html);                          //отправка запроса к серверу и получение ответной строки
                        ParseHtml(html, out string answer, out int ind);                //распознавание из строки индекса и ответа
                        ProcessAnswerAndShow(answer, ind);                              //обработка ответа и вывод в строку datagrid
                    }))
                    { IsBackground = true };
                thread.Start();
                i++;
                if (avTreads != -1)                                                     //если число доступных потоков задано
                    Thread.Sleep(
                        Math.Max(waitServ / avTreads, defWaitServ)                      //задержка до обработки следущей строки исходя из числа доступных потоков (с учётом минимальной стандартной задержки)
                        );
                else Thread.Sleep(
                    Math.Max(waitServ / curTreads, defWaitServ)                         //иначе => задержка до обработки следущей строки исходя из числа текущих потоков (с учётом минимальной стандартной задержки)
                    );
            }
            waitServ = defWaitServ;
            limitReached = false;
            
            FolderPathBtnIsEnabled = true; CheckPalindromeBtnIsEnabled = true;
            RaisePropertyChanged("FolderPathBtnIsEnabled");
            RaisePropertyChanged("CheckPalindromeBtnIsEnabled");
        }

        private void ProcessAnswerAndShow(string answer, int ind)
        {
            if (ind != -1)                                                  //если отсутствует ошибка
            {
                _dispatcher.Invoke(new Action(() =>
                {
                    while (SimTPalObservCollection.Count <= ind)
                        SimTPalObservCollection.Add(new SimilarityTPalItem { SimilarityTPalString = "<Ожидание сервера>" });
                }));
                bool waitContain = answer.Contains("wait=");
                if (waitContain)                                            //если сервер перегружен
                {
                    if (!limitReached)                                      //если сервер еще не перегружался
                    {
                        if (Int32.TryParse(answer.Substring(5), out int time))
                        {
                            TranslateAnswer(ref answer);                    //перевод ответа для вывода в datagrid
                            waitServ = time * 1000;                         //задать значение задержки отправки запросов
                            limitReached = true;                            //флаг: сервер был перегружен
                        };
                    };
                }
                else
                {
                    if (ind <= SimTPalObservCollection.Count && ind <= ObservCollectionForDG.Count && ObservCollectionForDG[ind].Procd == false)
                    {                                                       //отбрасываются обработанные строки и строки вне диапазона 
                        _dispatcher.Invoke(new Action(() =>
                        {
                            TranslateAnswer(ref answer);                    //перевод ответа для вывода в datagrid

                            SimTPalObservCollection[ind] = new SimilarityTPalItem { SimilarityTPalString = answer };
                            //вставка ответа в datagrid
                            ObservCollectionForDG[ind].Procd = true;        //пометить строку как обработанную
                        }));
                    };
                };
                RaisePropertyChanged("DGSimTPalItems");
            }
            //GC.Collect();
        }

        private delegate void delegateForInvoke();

        private static void ReceiveHtml(HttpWebRequest request, out string html)
        {
            html = "";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK) return;

                var stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                    html = HttpUtility.UrlDecode(html, Encoding.UTF8);
                }
                stream.Close();
                response.Close();
            }
            catch (System.Net.WebException) { html = ""; return; }
            
            request.Abort();
        }

        private static void ParseHtml(string htmlText, out string answer, out int ind)
        {
            answer = ""; ind = -1;
            var htmlM = htmlText.Split('<');
            bool succ = false;

            foreach (string tag in htmlM)
            {
                if (tag.Trim().StartsWith("input"))
                {
                    if (tag.Contains(@"""answer"""))
                        answer = ExtractValue(tag);
                    //else if (tag.Contains(@"""word"""))
                    //    word = ExtractValue(tag);
                    else if (tag.Contains(@"""index"""))
                        succ = Int32.TryParse(ExtractValue(tag),out ind);
                }
            }
            if (!succ) ind = -1;
        }

        private static void TranslateAnswer(ref string answer)
        {
            switch (answer.ToLower())
            {
                case "true":
                    answer = "Палиндром";
                    break;
                case "false":
                    answer = "Не палиндром";
                    break;
                default:
                    if (!answer.Contains("wait="))  //если ответ не содержит true, false или wait то вернуть error
                        answer = "error";
                    break;
            }
        }

        private static string ExtractValue(string text)
        {
            int begInd = text.IndexOf(@"value=""") + 7;
            int endInd = text.IndexOf(@"""", begInd);
            return text.Substring(begInd, endInd - begInd);
        }

        public string ShowOpenDialogForFolderPath()
        {
            using (CommonOpenFileDialog dialogDirectory = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                return _FolderPath = dialogDirectory.ShowDialog() == CommonFileDialogResult.Ok ? dialogDirectory.FileName : null;
            }
        }

        public int FillDataGridWithFolderPath() => FillDataGridWithFolderPath(_FolderPath);

        public int FillDataGridWithFolderPath(string folderPath)
        {
            ObservCollectionForDG.Clear();
            SimTPalObservCollection.Clear();
            try
            {
                List<string> filesname = Directory.GetFiles(folderPath, "*.txt").ToList<string>();
                foreach (var filePath in filesname)
                {
                    StreamReader stream = new StreamReader(filePath, Encoding.UTF8);
                    Guid guid = Guid.NewGuid();
                    string guidS = guid.ToString();
                    ObservCollectionForDG.Add(new FileDataItem { Text = stream.ReadToEnd().Replace("\r", "").Replace("\n", ""), Procd = false });
                }                
                RaisePropertyChanged("FolderPath"); 
                RaisePropertyChanged("DGFilesItems");
                RaisePropertyChanged("DGSimTPalItems");
                return 0;
            }
            catch { return -1; }            
        }
    }
}
