using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Linq;
using System.Management;


namespace BingIOD
{
    class Program
    {
        static void Main(string[] args)
        {
            //add something to check internet connection

            bool isconnected = CheckForInternetConnection();

            if (isconnected == false)
            {
                Console.WriteLine("No Internet connection found");
                Console.WriteLine("Connect to the internet and run the application again");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            BingURL();
            //im using console.readkey to keep thing open whil downloading the files
            //I need to fivure out how to use a task to make it wait
            Console.ReadKey();
        }

        //method for getting the URL
        private static async void BingURL()
        {
            // We can specify the region we want for the Bing Image of the Day.
            string strRegion = "en-US";
            int _numOfImages = 15;
            string strBingImageURL = string.Format("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n={0}&mkt={1}", _numOfImages, strRegion);
            string strJSONString = "";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(strBingImageURL));
            strJSONString = await response.Content.ReadAsStringAsync();

            ////write the json file for debugging
            //using (StreamWriter w = File.AppendText("c:\\temp\\BingIODJSON.log"))
            //{
            //    Log(strJSONString, w);
            //}

            //Split strJSONString on ,
            char[] delimiterChars = { ',' };
            string[] imgs = strJSONString.Split(delimiterChars);

            //now that it is split look at each line for a file name and download it
            foreach (string s1 in imgs)
            {
                string s2 = "jpg";
                bool b = s1.Contains(s2);
                if (b)
                {
                    //look for the JPS and it's path and trun it into a URL
                    string s3 = s1.Replace(":", "");
                    s3 = s3.Replace("url", "https://bing.com");
                    s3 = s3.Replace("\"", "");
                    //Console.WriteLine(s3);

                    //now pull the file name out of the URL
                    String fileName = Path.GetFileName(s3);
                    fileName = fileName.Substring(fileName.IndexOf('.') + 1);
                    int index = fileName.IndexOf("&");
                    fileName = fileName.Substring(0, index);
                    Console.WriteLine("Filename = " + fileName);

                    //the URL is now stored in s3 do I need to download it
                    Console.WriteLine("Downloading the file {0}", fileName);
                    try
                    {
                        string DlDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                        //Console.WriteLine(DlDir);
                        WebClient webClient = new WebClient();
                        string DLfile = DlDir + @"\" + fileName;
                        if (File.Exists(DLfile))
                        {
                            Console.WriteLine("File already exists!");
                            Thread.Sleep(1000);
                        }

                        if (!File.Exists(DLfile))
                        {
                            webClient.DownloadFile(s3, DlDir + @"\" + fileName);
                            Console.WriteLine("Download Completed");
                            Thread.Sleep(2000);
                        }
                   

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error Downloading File");
                        string DefaultLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        using (StreamWriter w = File.AppendText(DefaultLogFilePath + "\\BingIOD.log"))
                        {
                            Log("Error downloading file: " + fileName, w);
                            Log("URL: " + s3, w);
                            Log(ex.ToString(), w);
                        }

                        //Console.ReadKey();

                    }

                }

            }
            Thread.Sleep(5000);

            Environment.Exit(0);
            //Console.ReadKey();

        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n={0}&mkt={1}"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        //logging
        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine("{0}: {1}: {2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), logMessage);
        }

    }
}
