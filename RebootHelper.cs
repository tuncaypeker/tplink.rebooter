using System;
using System.IO;
using System.Net;
using System.Web;

namespace TpLink.Rebooter
{
    public class RebootHelper
    {
        public static string Reboot()
        {
            var currentIp = GetExternalIp();
            var username = "";
            var password = "";
            var auth = "Basic " + Base64Encoding(username + ":" + password);

            Uri target = new Uri("http://192.168.0.1/");

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("Authorization", HttpUtility.JavaScriptStringEncode(auth)) { Domain = target.Host });
            cookieContainer.Add(new Cookie("SubType", "pcSub") { Domain = target.Host });
            cookieContainer.Add(new Cookie("TPLoginTimes", "1") { Domain = target.Host });

            var request = (HttpWebRequest)WebRequest.Create("http://192.168.0.1/");
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookieContainer;
            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var regex = new System.Text.RegularExpressions.Regex("var session_id = \"(.*)?\";");
            if (regex.IsMatch(responseString))
            {
                var sessionId = regex.Match(responseString).Groups[1].Value;
                request = (HttpWebRequest)WebRequest.Create($"http://192.168.0.1/userRpm/reboot.htm?reboot=Reboot&session_id={sessionId}");
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = cookieContainer;
                response = (HttpWebResponse)request.GetResponse();

                //internet gidecek 10 sn bekle sonra 5 sn'de bir geldi mi die kontrol et
                System.Threading.Thread.Sleep(10 * 1000);

                while (!CheckForInternetConnection())
                    System.Threading.Thread.Sleep(2 * 1000);

                return GetExternalIp();
            }
            else
                return currentIp;  
        }

        public static string Base64Encoding(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string GetExternalIp()
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");
            return externalip;
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
