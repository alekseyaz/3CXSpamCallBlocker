using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace _3CXSpamCallBlocker
{
    class WhoCalling
    {
        public static bool ThisBadCall(string PhoneNumber)
        {
            string answer = GET("https://yandex.ru/suggest/suggest-ya.cgi", "part=" + HttpUtility.UrlEncode(PhoneNumber) + "&fact=1&v=4");
            Program.MyLogger.Trace($"{PhoneNumber} {answer}");

            string[] ListSpamCategories = GetListSpamCategories();

            foreach (string sc in ListSpamCategories)
            {
                int indexOfSubstring = answer.IndexOf(sc);
                if (indexOfSubstring != -1)
                {
                    return true;
                }
            }
            return false;
        }

        static string GET(string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url + "?" + Data);
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

        static string[] GetListSpamCategories()
        {
            string[] listSpamCategories;
            string filePathListSpamCategories = @".\BlacklistCategories.txt";

            if (!File.Exists(filePathListSpamCategories))
            {
                try
                {
                    listSpamCategories = new string[] { "Возможно, опросы от операторов связи"
                        , "Возможно, реклама услуг связи"
                        , "Возможно, нежелательный звонок"
                        , "Возможно, нежелательные звонки"
                        , "Возможно, злоумышленники"
                        , "Возможно, звонят операторы связи"
                        , "Возможно, звонят по поводу задолженности"
                        , "Возможно, звонят из банка по поводу задолженности"
                        , "Возможно, реклама"
                        , "Возможно, полезные звонки из банка"
                        , "Возможно, звонят, чтобы провести опрос"
                        , "Возможно, звонят из банка"
                        , "unknow" };
                    File.WriteAllLines(filePathListSpamCategories, listSpamCategories, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                listSpamCategories = File.ReadAllLines(filePathListSpamCategories);
            }
            return listSpamCategories;
        }
    }
}