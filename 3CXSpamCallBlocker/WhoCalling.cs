using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;

namespace _3CXSpamCallBlocker
{
    class WhoCalling
    {

        private static string GET(string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url + "?" + Data);
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

        private static string[] GetListSpamCategories()
        {

            string[] ListSpamCategories;

        
                string filePathBlacklistCategories = @".\BlacklistCategories.txt";


                if (!File.Exists(filePathBlacklistCategories))
                {
                    try
                    {
                        ListSpamCategories = new string[] { "Возможно, опросы от операторов связи"
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
                        File.WriteAllLines(filePathBlacklistCategories, ListSpamCategories, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                }
                else
                {
                    ListSpamCategories = File.ReadAllLines(filePathBlacklistCategories);
                }


            return ListSpamCategories;


        }

        static bool ThisBadCall(string PhoneNumber)
        {
            string answer = GET("https://yandex.ru/suggest/suggest-ya.cgi", "part=" + HttpUtility.UrlEncode(PhoneNumber) + "&fact=1&v=4");
            //logger.Trace($"{getPhoneNumber} {answer}");


            foreach (string spamCategory in GetListSpamCategories())
            {
                int indexOfSubstring = answer.IndexOf(spamCategory);
                if (indexOfSubstring != -1)
                {
                    return true;
                }
            }
            return false;
        }






    }
}
