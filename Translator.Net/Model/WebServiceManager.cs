using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Translator.Net.Model
{
    public abstract class Loader<T>
    {
        protected string Url { get; set; }
        protected string FromCulture { get; set; }
        protected string ToCulture { get; set; }

        private HttpClient client = new HttpClient();
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<string> GetAsync(string url)
        {
            await semaphoreSlim.WaitAsync();
            string payload = string.Empty;
            try
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    payload = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }     
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return payload;
        }

        public abstract Task<List<T>> Load();
    }

    public class WebServiceManager
    {
        public Task<List<T>> Load<T>(Loader<T> loader)
        {
            return loader.Load();
        }
    }
}

public static class StringExtentions
{
    public static string TrimOuter(this string content, string trim1, string trim2)
    {
        int first = content.IndexOf(trim1);
        int last = content.LastIndexOf(trim2);
        return content.Substring(first, last - first + 1);
    }

    public static string FindJSON(this string content, string startingIndicator)
    {
        int startingPosition = content.IndexOf(startingIndicator);
        string newContent = content.Substring(startingPosition + startingIndicator.Length);

        int bracketCount = 0;
        string jsonString = string.Empty;
        for (int i = 0; i < newContent.Length; i++)
        {
            string str = newContent[i].ToString();
            if (str == "{")
                bracketCount++;
            else if (str == "}")
                bracketCount--;

            if (bracketCount == 0 && i > 0 && string.IsNullOrEmpty(jsonString))
            {
                jsonString = newContent.Substring(0, i + 1);
                break;
            }
        }
        return jsonString;
    }
}
