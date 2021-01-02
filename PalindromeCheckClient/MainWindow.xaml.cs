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
        readonly string URIstr = "http://127.0.0.1:8080/";
        List<string> guidL = new List<string>();
        //ListBox isPali = new ListBox();
        //List<string> isPaliL = new List<string>();
        MainWindowViewModel mWinVM = new MainWindowViewModel();

        public string URITbxValue
        {
            set { URITbx.Text = value; }
            get { return URITbx.Text; }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mWinVM;
            mWinVM.URI = URIstr;            
        }

        private void CheckPalindromeBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mWinVM.URI.Trim() == "" || (mWinVM.URI.Trim().IndexOf("http://") == -1 && mWinVM.URI.Trim().IndexOf("https://") == -1))
                return;
            int filesN = PaliFilesLBox.Items.Count;
            IsPaliLBox.Items.Clear();
            while (IsPaliLBox.Items.Count<filesN)
                IsPaliLBox.Items.Add("#");
            FolderPathBtn.IsEnabled = false; CheckPalindromeBtn.IsEnabled = false;
            var thread = new Thread(
                StartSendFiles
                    ) { IsBackground = true };
            thread.Start();
        }

        private void StartSendFiles()
        {
            int i = 0;
            int errN = 0;
            int threads = 1;
            bool maxThreads = false;
            double waitServ = 0.3;
            while (true)
            {
                if (IsPaliLBox.Items[i].ToString() != "#")
                {
                    if (errN == 0) break;
                    continue;
                };

                var request = (HttpWebRequest)HttpWebRequest.Create(mWinVM.URI);
                request.Method = "POST";
                string postData = "IsPalindrome=&word=" + PaliFilesLBox.Items[i].ToString() + "&index=" + guidL[i]; //PaliFilesLBox.Items[i].ToString()
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "text/html";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                _ = Dispatcher.BeginInvoke(new Action(() =>
                {
                    int ind; string answer;
                    ReceiveAnswer(request, out ind, out answer);
                    answer = TranslateAnswer(answer);
                    if (answer != "error")
                    {
                        IsPaliLBox.Items[ind] = answer;
                        threads++;
                    }
                    else
                    {
                        if (answer.Contains("wait="))
                        {
                            waitServ = Convert.ToInt32(answer.Substring(5));
                            maxThreads = true; threads--;
                        }
                        errN++;
                    }
                }));
                //request.BeginGetResponse(new AsyncCallback(OnGetHtmlResponse), request);    
                i++;
                if (i >= PaliFilesLBox.Items.Count && errN == 0)
                    break;
                else if (errN != 0)                    
                {
                    i = 0;
                    errN = 0;
                }

                if (maxThreads)
                    Thread.Sleep(Convert.ToInt32(waitServ * 1050 / threads));
                else
                    Thread.Sleep(Convert.ToInt32(waitServ * 1050));
            }
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                FolderPathBtn.IsEnabled = true; CheckPalindromeBtn.IsEnabled = true;
            }));
        }

        private delegate void delegateForInvoke();

        private void ReceiveAnswer(HttpWebRequest request, out int index, out string answer)//OnGetHtmlResponse(IAsyncResult result)
        {
            string guidS = "";//, word = ""; 
            answer = ""; index = -1;
            //var request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();//.EndGetResponse(result);
            
            if (response.StatusCode != HttpStatusCode.OK) return;

            string html;
            var stream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
                html = HttpUtility.UrlDecode(html, Encoding.UTF8);
            }
            stream.Close();
            response.Close();
            request.Abort();
            var htmlM = html.Split('<');
            

            foreach (string tag in htmlM)
            {
                if (tag.Trim().StartsWith("input"))
                {
                    if (tag.Contains(@"""answer"""))//answer = tag.Replace(" ", "").ToLower();
                        answer = ExtractValue(tag);
                    //else if (tag.Contains(@"""word"""))
                    //    word = ExtractValue(tag);
                    else if (tag.Contains(@"""index"""))
                        guidS = ExtractValue(tag);
                }
            }
            index = guidL.IndexOf(guidS);
        }

        private string TranslateAnswer(string answer)
        {
            switch (answer.ToLower())
            {
                case "true":
                    return "Палиндром";                    
                    break;
                case "false":
                    return "Не палиндром";
                    break;
                default:               
                    return "error";                        
                    break;
            }
        }

private string ExtractValue(string text)
        {
            int begInd = text.IndexOf(@"value=""") + 7;
            int endInd = text.IndexOf(@"""", begInd);
            return text.Substring(begInd, endInd - begInd);
        }

        private void FolderPathBtn_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialogDirectory = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                FolderPathTbx.Text = dialogDirectory.ShowDialog() == CommonFileDialogResult.Ok ? dialogDirectory.FileName : null;
            }
        }

        private void FolderPathTbx_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LoadFilesInLBox(FolderPathTbx.Text);
        }

        private void LoadFilesInLBox(string folderPath)
        {
            List<string> filesname = Directory.GetFiles(folderPath, "*.txt").ToList<string>();
            //List<string> files = new List<string> { };
            foreach (var filePath in filesname)
            {
                StreamReader stream = new StreamReader(filePath, Encoding.UTF8);
                PaliFilesLBox.Items.Add(stream.ReadToEnd().Replace("\r", "").Replace("\n", ""));
                Guid guid = Guid.NewGuid();
                string guidS = guid.ToString();
                guidL.Add(guidS);
            }
        }

    }
}
