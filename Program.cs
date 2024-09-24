using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace SlackFileDownload
{
    // Extensions to string
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
            ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);
            int position = startIndex-1;
            for (int i = 1; i <= n; i++)
            {
                position = str.IndexOf(value, position + 1, count - (position + 1 - startIndex));
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
            ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);
            int position = startIndex-1;
            for (int i = 1; i <= n; i++)
            {
                position = str.IndexOf(value, position + 1, count - (position + 1 - startIndex));
                if (position < 0) return position;
            }
            return position;
        }
    }

    // JSON schema
    public class Message
    {
        public string type { get; set; }
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
            return string.Compare(x.url_private_download, y.url_private_download);
        }
    }

    // Main program
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize FileMessage store
            var fms = new SortedSet<FileMessage>(new FileMessageComparer());

            // Set current directory
            string target = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(target);

            // Find all downloadable files
            string[] directories = Directory.GetDirectories(".");
            foreach (string dir in directories)
            {
                string[] jsons = Directory.GetFiles(dir, "*.json");
                foreach (string json in jsons)
                {
                    Console.WriteLine("Processing file: {0}", json);
                    string jsonString = File.ReadAllText(json);
                    var messages = JsonSerializer.Deserialize<Message[]>(jsonString);
                    foreach (var m in messages)
                    {
                        if (m.type == @"message" && m.files is not null)
                        {
                            foreach (var fm in m.files)
                            {
                                if (fm.url_private_download is not null)
                                {
                                    var success = fms.Add(fm);
                                    Console.Write(success ? "Added: " : "Failed: ");
                                    Console.WriteLine(fm.url_private_download);
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Total files: {0}", fms.Count);

            // Download files
            if (fms.Count > 0)
            {
                target = Directory.GetCurrentDirectory() + "\\files"; // ensure absolute path
                Directory.CreateDirectory(target);

                var client = new HttpClient();
                foreach (var fm in fms)
                {
                    string url = fm.url_private_download;
                    Console.Write("Processing URL: {0}", url);
                    int dirStart = url.IndexOfNth('/', 4) + 1;
                    int dirEnd = url.IndexOf('/', dirStart);
                    string dirName = url[dirStart..dirEnd];

                    if (string.IsNullOrEmpty(fm.name))
                    {
                        int fileStart = url.IndexOf('/', dirEnd + 1) + 1;
                        int fileEnd = url.IndexOf('?', fileStart);
                        fm.name = url[fileStart..fileEnd];
                    }

                    Directory.SetCurrentDirectory(target);
                    Directory.CreateDirectory(dirName);
                    Directory.SetCurrentDirectory(dirName);

                    using (var httpStream = client.GetStreamAsync(url).Result)
                    using (var fileStream = new FileStream(fm.name, FileMode.Create))
                    httpStream.CopyTo(fileStream);

                    Console.WriteLine(@" (DONE)");
                }
            }
            Console.WriteLine(@"All done.");
        }
    }
}
