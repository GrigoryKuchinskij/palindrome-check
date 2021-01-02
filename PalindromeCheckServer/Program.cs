using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PalindromeCheckServer
{
    class Program
    {
        internal static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        static void Main(string[] args)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Текущая ос не поддерживается!");             //если текущая ос не поддерживается
                Console.ReadKey();
                return;
            }
            if (!IsRunAsAdmin())
            { 
                Console.WriteLine("Запустите от имени администратора!");
                Console.ReadKey();
                return;
            }
            CheckPalindromeServer server = new CheckPalindromeServer();
            server.maxNumbOfRequests = 4;

            string Ip = "", Port = "", addr= "";
            bool exit = false;
            foreach (string arg in args)
            {
                if (arg.ToLower().StartsWith("addr-")) 
                    Ip = arg.Substring(5);
                if (arg.ToLower().StartsWith("port-"))
                    Port = arg.Substring(5);
                if (arg.ToLower().StartsWith("maxreq-"))
                    server.maxNumbOfRequests = Convert.ToInt32(arg.Substring(7));
                if (arg == "skip-punctuation") server.skipPn = true;
            }
            if (Ip == "" && Port == "") addr = "+:8080"; 
            else if (Ip == "") addr = "+:" + Port;
            else if (Port == "") addr = Ip;
            else addr = Ip + ":" + Port;
            addr = @"http://" + addr + "/";
            
            server.StartListener(addr);
            Console.WriteLine(@"Сервер запущен по адресу " + addr);
            if (server.skipPn) Console.WriteLine("Пропуск знаков припинания в палиндроме: вкл"); 
            else Console.WriteLine("Пропуск пробелов в палиндроме: выкл");
            Console.WriteLine(@"Доступно " + server.maxNumbOfRequests + " поток(-а/-ов) для обрабатываемых запросов");
            Console.WriteLine();

            Task.Run(() => server.StartClientFlow());

            while (!exit)
            {
                Console.WriteLine(@"[x]-Выход | [s]-Пропуск знаков | *число*-количество потоков");
                string cs = Console.ReadLine().ToLower();
                switch (cs)
                {
                    case "x":
                        server.StopListener();
                        exit = true;
                        break;
                    case "s":
                        server.skipPn = !server.skipPn;
                        if (server.skipPn) 
                            Console.WriteLine("Пропуск знаков припинания в палиндроме: вкл"); 
                        else 
                            Console.WriteLine("Пропуск знаков припинания в палиндроме: выкл");
                        break;
                    default:
                        try 
                        {
                            server.maxNumbOfRequests = Convert.ToInt32(cs);
                            Console.WriteLine(@"Доступно " + server.maxNumbOfRequests + " поток(-а/-ов) для обрабатываемых запросов");
                        } catch { };
                        break;
                }            
            }         
        }        
    }

    class CheckPalindromeServer
    {
        public bool skipPn = false;
        public int maxNumbOfRequests = 2;
        private int curNumbOfRequests = 0;
        private readonly int sleepDur = 1;
        private readonly string[] httpPage = {
            @"<!DOCTYPE HTML><html><head></head><body>
            <form method=""post"" action=""check-palindrome"">
            IsPalindrome: <input type=""text"" readonly=""true"" size=""5"" name=""answer"" value=""",
            @"""><br><input type=""text"" name=""word"" value=""",
            @"""><br><input type=""text"" readonly=""true"" name=""index"" value=""",
            @"""><p><input type=""submit"" value=""send""></p>
            </form></body></html>" };
        private HttpListener listener = new HttpListener();
        public void StartListener(string prefix)
        {
            if (listener.IsListening == true) 
            { Console.WriteLine("Сервер уже запущен!"); return; };
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            //запускаем север
            listener.Start();
            Console.WriteLine("Запуск сервера.");
        }
        public void StopListener()
        {
            listener.Stop();
            listener.Prefixes.Clear();
        }

        public void StartClientFlow()
        {
            if (listener.IsListening == false)
            { Console.WriteLine("Сервер не запущен!"); return; };
            Console.WriteLine(@"Нагрузка:        Состояние запроса:");
            while (listener.IsListening)                                //пока сервер запущен
            {
                HttpListenerContext context = listener.GetContext();    //ожидаем входящие запросы

                if (curNumbOfRequests >= maxNumbOfRequests)             //если превышено количество обрабатываемых запросов
                {
                    SendResponseData(context.Response, httpPage[0] + "wait=" + sleepDur + httpPage[1] + httpPage[2] + httpPage[3]);
                    Console.WriteLine(curNumbOfRequests + @"/" + maxNumbOfRequests + "        rejected");
                    continue;
                }
                else 
                    Task.Run(() => CheckPalindromeInContext(context));
            }
            curNumbOfRequests = 0;                                      //при остановке сервера сбросить счетчик
            Console.WriteLine("Сервер остановлен!");
        }

        private void CheckPalindromeInContext(HttpListenerContext context)
        {
            ++curNumbOfRequests;
            try
            {
                string reqWord="";
                string reqGUID = "";
                char[] punctuation = { '.', ',', '!', '?', ';', ':', '"', '-' };
                string reqText = ReadRequestData(context.Request);
                Console.WriteLine(curNumbOfRequests + @"/" + maxNumbOfRequests + "        received");
                string[] reqvals = reqText.Split('&');
                foreach (string reqval in reqvals)
                {
                    if (reqval.StartsWith("word="))                    
                        reqWord = reqval.Replace("word=", "");
                    if (reqval.StartsWith("index="))
                        reqGUID = reqval.Replace("index=", "");
                }
                Console.WriteLine(reqWord);
                reqText = reqWord.ToLower();//.Replace("\r","").Replace("\n", "");
                if (skipPn == true)
                {
                    reqText = new string(reqText.Select(s => (punctuation.Contains(s) ? ' ' : s)).ToArray());
                    //string result = string.Join(" ", temp.Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries));
                    reqText = reqText.Replace(" ", "");
                }                
                bool isPol = reqText.IsPalindrome();
                //формируем ответ сервера:
                Thread.Sleep(sleepDur * 1000);
                SendResponseData(context.Response, httpPage[0] + isPol.ToString() + httpPage[1] + reqWord.Replace("\"", "") + httpPage[2] + reqGUID + httpPage[3]);
                Console.WriteLine(curNumbOfRequests + @"/" + maxNumbOfRequests + "        processed");
            }
            catch (HttpException ex) { Console.WriteLine(@"Ошибка обработки запроса!" + ex.Message); }
            catch { SendResponseData(context.Response, httpPage[0] + "Error" + httpPage[1] + httpPage[2] + httpPage[3]); }
            --curNumbOfRequests;
        }

        private string ReadRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody) return "";
            using (Stream body = request.InputStream)
            {
                using (StreamReader reader = new StreamReader(body))
                {
                    string text = reader.ReadToEnd();
                    text = HttpUtility.UrlDecode(text, Encoding.UTF8);
                    return text;
                }
            }   
        }
        private void SendResponseData(HttpListenerResponse response, string data)
        {
            string responseString = data;
            response.ContentType = "text/html; charset=UTF-8";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            using (Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }            
        }
    }

    public static class StringExtensions
    {
        public static bool IsPalindrome(this string text)
        {
            int Len = text.Count();
            if (Len <= 1) return false;
            int halfLen = (int)Math.Truncate(Len / (double)2);
            char[]  directHalf = text.ToCharArray(0, halfLen),
                    reversedHalf = text.ToCharArray(Len - halfLen, halfLen);                    //разделить текст на две равные части отбросив символ по середине (если он есть)
            Array.Reverse(reversedHalf);                                                    //перевернуть вторую половину
            return (new string(directHalf).Replace(new string(reversedHalf), "") == "");    //Удалить совпадающие строки первой и второй половины. 
                                                                                            //Если останется пустая строка, то это палиндром.
        }
    }
}
