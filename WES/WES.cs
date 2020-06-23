using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class WES
{
    private static readonly HttpClient client = new HttpClient();
    private static string Website = "http://localhost/";
    private static string WebsitePath = "";

    public static async Task<string> ExecuteAsync(string raw)
    {
        if (raw.StartsWith("help"))
        {
            return "Use 'open <url>' to start a session with a website\n" +
                    "Use 'cd <path>' to navigate a websites files.\n" +
                    "Use 'pwd' to print working directory\n" +
                    "Use 'post <data>' to send a POST request\n" +
                    "Use 'get <data>' to send a GET request\n" +
                    "Use 'file <localPath>' to dowbload a file.\n";
        }
        else if (raw.StartsWith("open"))
        {
            string website = raw.Substring(4).Trim();
            if (!website.EndsWith("/"))
            {
                website += "/";
            }
            Website = website;
            return Website;
        }
        else if (raw.StartsWith("cd"))
        {
            string path = raw.Substring(2).Trim();
            if (path.StartsWith("/")) {
                path = path.Substring(1);
            }
            WebsitePath = path;
            return Website + WebsitePath;
        }
        else if (raw.StartsWith("pwd"))
        {
            return Website + WebsitePath;
        }
        else if (raw.StartsWith("post"))
        {
            var postValues = new Dictionary<string, string>();
            string[] valuesArray = raw.Substring(4).Trim().Split('&');
            foreach (string value in valuesArray)
            {
                string[] valuePair = value.Split('=');
                postValues.Add(valuePair[0], valuePair[1]);
            }
            var content = new FormUrlEncodedContent(postValues);

            var response = await client.PostAsync(Website + WebsitePath, content);

            return await response.Content.ReadAsStringAsync();
        }
        else if (raw.StartsWith("get"))
        {
            return await client.GetStringAsync(Website + WebsitePath + "?" + raw.Substring(3).Trim());
        }
        else if (raw.StartsWith("file"))
        {
            WebClient Client = new WebClient();
            Client.DownloadFile(Website + WebsitePath, raw.Substring(4).Trim());
            return "File saved.";
        }
        else
        {
            return "Invalid";
        }
    }
}