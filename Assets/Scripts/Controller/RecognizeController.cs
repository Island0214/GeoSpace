using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public class RecognizeController : MonoBehaviour
{
    WritingPanel writingPanel;
    PenBehaviour penBehaviour;

    private static readonly string clientId =  "tgalsFoilzd4LTyNDf4Mq7mp";

    private static readonly string clientSecret  = "FNfIOtGiAIKnib2FrHHeAaj8Aog98edZ";

    public void Init(WritingPanel writingPanel)
    {
        this.writingPanel = writingPanel;
    }

    public string GetRecognizeResult(string base64)
    {
        ///TODO
        return "s";
        try
    }



        public static string GetAccessToken()
        {
            string url = "https://aip.baidubce.com/oauth/2.0/token";
            var list = new List<KeyValuePair<string, string>>
                           {
                               new KeyValuePair<string, string>("grant_type", "client_credentials"),
                               new KeyValuePair<string, string>("client_id", clientId),
                               new KeyValuePair<string, string>("client_secret", clientSecret)
                           };
            var data = new List<string>();
            foreach (var pair in list)
                data.Add(pair.Key + "=" + pair.Value);
            return HttpGet(url, string.Join("&", data.ToArray()));
        }





        public static string HttpGet(string url, string data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url + (data == "" ? "" : "?") + data);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Stream stream = response.GetResponseStream();
                string s = null;
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                    {
                        s = reader.ReadToEnd();
                        reader.Close();
                    }
                    stream.Close();
                }
                return s;
            }
        }
 
        public static string HttpPost(string url, string data)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(data);
            Stream stream = request.GetRequestStream();
            var writer = new StreamWriter(stream, Encoding.GetEncoding("gb2312"));
            writer.Write(data);
            writer.Close();
 
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Stream res = response.GetResponseStream();
                if (res != null)
                {
                    var reader = new StreamReader(res, Encoding.GetEncoding("utf-8"));
                    string retString = reader.ReadToEnd();
                    reader.Close();
                    res.Close();
                    return retString;
                }
            }
            return "";
        }


}