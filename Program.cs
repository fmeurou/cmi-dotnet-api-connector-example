using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace cmiConnectDotNet
{
    class Program
    {
        

        static void Main(string[] args)
        {
            if(args.Length < 2) {
                Console.WriteLine("Domain (www.conjectmi.net) and username required.");
                return;
            }
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
            Connector connector = new Connector();
            connector.Referer = args[0];
            Console.WriteLine("password required:");
            String password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    Console.Write("\b");
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            if (password != "") {
                Console.WriteLine(connector.connect(
                    args[1],
                    password
                ));

            } else {
                Console.WriteLine("invalid password");
            }
            //Console.WriteLine(connector.login(args[0], args[1]));

        }
    }

    class Connector {
        String CsrfToken {get; set;}
        public String Referer {get; set;}
        static String CsrfUrl = "api/csrftoken/";
        static String LoginUrl = "ng/login/";
        static String ProjectsUrl = "ng/projects/";
        CookieContainer cookieContainer = new CookieContainer();

        public String connect(String email, String password)   {
            try {
                CsrfToken = getCsrf();
                Console.WriteLine("CsrfToken: " + CsrfToken);
                login(email, password);
                var ProjectList = projects();
                return ProjectList;
            } catch(System.Net.Http.HttpRequestException)  {
                return "Error querying server";
            }
        }

        public String getCsrf()   {
            var url = "https://" + Referer + "/" + CsrfUrl;
            var client = (HttpWebRequest)WebRequest.Create(new Uri(url));
            client.Headers.Add("X-Requested-With","XMLHttpRequest");
            client.Headers.Add("Referer", Referer);
            client.CookieContainer = cookieContainer;
            Stream data = client.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader (data);
            string s = reader.ReadToEnd ();
            data.Close ();
            reader.Close ();
            return s;
        }

        public String login(String email, String password)   {
            var url = "https://" + Referer + "/" + LoginUrl;
            var client = (HttpWebRequest)WebRequest.Create(new Uri(url));
            client.Accept = "application/json";
            client.ContentType = "application/x-www-form-urlencoded";
            client.Method = "POST";
            client.Headers.Add("X-Requested-With","XMLHttpRequest");
            client.Headers.Add("Referer", Referer);
            client.Headers.Add("X-CSRFToken", CsrfToken);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            client.CookieContainer = cookieContainer;

            var vm = new { email = email, password = password, csrftoken = CsrfToken };
            var dataString = JsonConvert.SerializeObject(vm);
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(dataString);
            
            Stream newStream = client.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();
            try {
                var response = client.GetResponse();

                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();
                return content;
            } catch(System.Net.WebException e)    {
                Console.WriteLine(e.Message);
                Console.WriteLine(((HttpWebResponse)e.Response).StatusCode);
                Stream data = ((HttpWebResponse)e.Response).GetResponseStream();
                StreamReader reader = new StreamReader (data);
                var s = reader.ReadToEnd ();
                Console.WriteLine(s);
                return "Error reading server response";
            }
        }

        public String projects()   {
            var url = "https://" + Referer + "/" + ProjectsUrl;
            var requestData = "?per_page=20&search=&page=1";
            var client = (HttpWebRequest)WebRequest.Create(new Uri(url + requestData));
            client.Accept = "application/json";
            client.ContentType = "application/x-www-form-urlencoded";
            client.Method = "GET";
            client.Headers.Add("X-Requested-With","XMLHttpRequest");
            client.Headers.Add("Referer", Referer);
            client.CookieContainer = cookieContainer;

            
            try {
                var response = client.GetResponse();

                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();
                return content;
            } catch(System.Net.WebException e)    {
                Console.WriteLine(e.Message);
                Console.WriteLine(((HttpWebResponse)e.Response).StatusCode);
                Stream data = ((HttpWebResponse)e.Response).GetResponseStream();
                StreamReader reader = new StreamReader (data);
                var s = reader.ReadToEnd ();
                Console.WriteLine(s);
                return "Error reading server response";
            }
            
        }
    }
}
