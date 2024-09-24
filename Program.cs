using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlackFileDownload
{
    public static class Extensions
    {
        public static int IndexOfNth (this string str, string value, int n)
        {
            return str.IndexOfNth(value, n, 0, str.Length);
        }
        public static int IndexOfNth (this string str, string value, int n, int startIndex)
        {
            return str.IndexOfNth(value, n, startIndex, str.Length);
        }
        public static int IndexOfNth (this string str, string value, int n, int startIndex, int count)
        {
            if (n < 1) throw new ArgumentOutOfRangeException("n cannot be less than 1");
            int position = startIndex-1;
            for (int i = 1; i <= n; i++)
            {
                position = str.IndexOf(value, position+1, count-(position+1 - startIndex));
                if (position < 0) return position;
            }
            return position;
        }
        public static int IndexOfNth (this string str, char value, int n)
        {
            return str.IndexOfNth(value, n, 0, str.Length);
        }
        public static int IndexOfNth (this string str, char value, int n, int startIndex)
        {
            return str.IndexOfNth(value, n, startIndex, str.Length);
        }
        public static int IndexOfNth (this string str, char value, int n, int startIndex, int count)
        {
            if (n < 1) throw new ArgumentOutOfRangeException("n cannot be less than 1");
            int position = startIndex-1;
            for (int i = 1; i <= n; i++)
            {
                position = str.IndexOf(value, position+1, count-(position+1 - startIndex));
                if (position < 0) return position;
            }
            return position;
        }
    }
    public class Message
    {
        public string item_type { get; set; }
        public FileMessage item { get; set; }
        public FileMessage[] files { get; set; }
    }
    public class FileMessage
    {
        public string name { get; set; }
        public string url_private_download { get; set; }
    }
    public class FileMessageComparer : IComparer<FileMessage>
    {
        public int Compare(FileMessage x, FileMessage y)
        {
            return String.Compare(x.url_private_download, y.url_private_download);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var fms = new SortedSet<FileMessage>(new FileMessageComparer());
            string target = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(target);
            string[] directories = Directory.GetDirectories(".");
            foreach (string dir in directories) {
                string[] jsons = Directory.GetFiles(dir, "*.json");
                foreach (string json in jsons)
                {
                    Console.WriteLine(@"Processing file: " + json);
                    string jsonString = File.ReadAllText(json);
                    var messages = JsonSerializer.Deserialize<Message[]>(jsonString);
                    foreach (Message m in messages)
                    {
                        if (m.item_type == "F" && m.item is not null)
                        {
                            var success = fms.Add(m.item);
                            Console.Write(success ? "Added:  " : "Failed: ");
                            Console.WriteLine(m.item.url_private_download);
                        }
                        if (m.files is not null)
                        {
                            foreach (FileMessage fm in m.files)
                            {
                                var success = fms.Add(fm);
                                Console.Write(success ? "Added:  " : "Failed: ");
                                Console.WriteLine(fm.url_private_download);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Total files: {0}", fms.Count);
            if (fms.Count > 0)
            {
                WebClient client = new WebClient();
                foreach (FileMessage fm in fms)
                {
                    string url = fm.url_private_download;
                    Console.Write(@"Processing URL: " + url);
                    int dirStart = url.IndexOfNth('/',4)+1;
                    int dirEnd = url.IndexOf('/', dirStart);
                    string dirName = url.Substring(dirStart, dirEnd-dirStart);
                    if (String.IsNullOrEmpty(fm.name))
                    {
                        int fileStart = url.IndexOf('/', dirEnd+1)+1;
                        int fileEnd = url.IndexOf('?', fileStart);
                        fm.name = url.Substring(fileStart, fileEnd-fileStart);
                    }
                    Directory.SetCurrentDirectory(target);
                    string dirPath = @"files\" + dirName;
                    Directory.CreateDirectory(dirPath);
                    Directory.SetCurrentDirectory(dirPath);
                    client.DownloadFile(url, fm.name);
                    Console.WriteLine(@" (DONE)");
                }
            }
            Console.WriteLine(@"All done.");
        }
    }
}
