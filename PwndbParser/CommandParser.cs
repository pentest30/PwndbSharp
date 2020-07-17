using CommandDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Extreme.Net;
using HtmlAgilityPack;

namespace PwndbParser
{
    public class CommandParser
    {
        [Command(Name = "pwndb")]
        public async Task<int> ParseAsync(
            [Operand(Name = "d",Description = "Domain name")]
            string domain,
            [Option(ShortName = "u", Description = "Username")]
            string userName,
            [Option(ShortName = "t", Description = "Time out -default 60 000 milliseconds")]
            int? torPort,
            [Option(ShortName = "p", Description = "Tor port - default port : 9150")]
            int? timeOut,
            [Option(ShortName = "o", Description = "Output file")]
            string outputFile)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            cts.CancelAfter(timeOut?? 60000);
            token.ThrowIfCancellationRequested();
            var task = Task.Run(async () =>
            {
                var result = await PostQueryAsync(domain, userName, torPort).ConfigureAwait(false);
                var resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                await ParseResultAsync(resultContent, outputFile).ConfigureAwait(false);

            }, token);
            await task.ConfigureAwait(false);
            return 0;
        }

        private static async Task ParseResultAsync(string resultContent,string outputFile)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(resultContent);
            HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//pre");
            var listOfPasswords = new List<string>();
            var listOfEmails = new List<string>();
            foreach (HtmlNode link in collection)
            {
                var data = link.FirstChild.InnerHtml.Split("\n").Skip(1);
                string lastUser = "";
                foreach (var s in data)
                {
                    var innerData = s.Split("\n");
                   
                    
                    foreach (var s1 in innerData)
                    {
                        if (!s1.Contains("=>")) 
                            continue;
                        if (s1.Split("=>")[0].Contains("password"))
                            listOfPasswords.Add(s1.Split("=>")[1].Trim());
                        if (s1.Split("=>")[0].Contains("luser"))
                            lastUser = s1.Split("=>")[1];
                        if (s1.Split("=>")[0].Contains("domain"))
                            listOfEmails.Add(lastUser+"@" + s1.Split("=>")[1].TrimStart());
                        Console.WriteLine(s1);
                    }
                }

                if (listOfPasswords.Any())
                    await WriteResultsToFileAsync(outputFile, listOfPasswords, "passwords.txt").ConfigureAwait(false);
                if (listOfEmails.Any())
                    await WriteResultsToFileAsync(outputFile, listOfEmails, "emails.txt").ConfigureAwait(false);

            }
        }

        private static async Task WriteResultsToFileAsync(string outputFile, List<string> list, string fName)
        {
            var curDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var filePath = outputFile != null
                ? curDir + Path.DirectorySeparatorChar + outputFile
                : curDir + Path.DirectorySeparatorChar + fName;
            await using (StreamWriter file = new StreamWriter(filePath))
            {
                foreach (string line in list.Skip(1))
                    await file.WriteLineAsync(line).ConfigureAwait(false);
            }
        }

        private static async Task<HttpResponseMessage> PostQueryAsync(string domain, string userName, int? port)
        {
            var socksProxy = new Socks5ProxyClient("127.0.0.1", port ?? 9150);
            var url = "http://pwndb2am4tzkvold.onion/";
            var handler = new ProxyHandler(socksProxy);
            var httpClient = new HttpClient(handler);
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("luser", userName),
                new KeyValuePair<string, string>("domain", domain),
                new KeyValuePair<string, string>("luseropr", "1"),
                new KeyValuePair<string, string>("domainopr", "1"),
                new KeyValuePair<string, string>("submitform", "em"),
            });
            try
            {
                Console.WriteLine("Connecting to pwndb service on tor network...\n");
                var result = await httpClient.PostAsync(url, data).ConfigureAwait(false);
                return result;

            }
            catch (Exception e)
            {
                Console.WriteLine("Can't connect to service! restart tor service and try again");
            }

            return new HttpResponseMessage();
        }
    }
}
