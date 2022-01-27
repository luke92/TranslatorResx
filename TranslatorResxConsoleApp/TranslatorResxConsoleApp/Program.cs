using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TranslatorResxConsoleApp
{
    internal class Program
    {
		static string _apiKey = "";
        static void Main(string[] args)
        {
			//var inputFile = @"C:\Users\Luk3\Desktop\Global.resx";
			var inputFile = System.Configuration.ConfigurationManager.AppSettings["inputFile"];
			//var outputFile = @"C:\Users\Luk3\Desktop\Global.fr-FR.resx";
			var outputFile = System.Configuration.ConfigurationManager.AppSettings["outputFile"];
			
			var fromLanguage = System.Configuration.ConfigurationManager.AppSettings["fromLanguage"];
			var toLanguage = System.Configuration.ConfigurationManager.AppSettings["toLanguage"];

			_apiKey = System.Configuration.ConfigurationManager.AppSettings["API_KEY_Azure_Cognitive_Services"];

			var inputXml = File.ReadAllText(inputFile);
			var xDoc = new XmlDocument();
			xDoc.LoadXml(inputXml);
			var dataNodes = xDoc.SelectNodes("//data");
			foreach (XmlNode dataNode in dataNodes)
			{
				var fromWord = dataNode.SelectSingleNode("value").InnerText;
				Console.WriteLine(fromWord);
				//var toWord = Translate(fromWord, "es", "fr");
				var toWord = Translate(fromWord, fromLanguage, toLanguage);
				Console.WriteLine(toWord);
				dataNode.SelectSingleNode("value").InnerText = toWord;
			}
			File.WriteAllText(outputFile, xDoc.InnerXml);
		}

		public static string Translate(string text, string fromLanguage, string toLanguage)
		{
			try
			{
				var strKey = _apiKey;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 |
													   SecurityProtocolType.Tls |
													   SecurityProtocolType.Tls11 |
													   SecurityProtocolType.Tls12;
				var url = string.Format("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={0}&from={1}", toLanguage, fromLanguage);
				var wc = new WebClient();
				wc.Headers["Ocp-Apim-Subscription-Key"] = strKey;
				wc.Headers["Ocp-Apim-Subscription-Region"] = "westeurope";
				wc.Encoding = Encoding.UTF8;
				var jPost = new[] { new { Text = text } };
				var post = JsonConvert.SerializeObject(jPost, Newtonsoft.Json.Formatting.Indented);
				wc.Headers[HttpRequestHeader.ContentType] = "application/json";
				var json = "";
				try
				{
					json = wc.UploadString(url, "POST", post);
				}
				catch (WebException exception)
				{
					string strResult = "";
					if (exception.Response != null)
					{
						var responseStream = exception.Response.GetResponseStream();
						if (responseStream != null)
						{
							using (var reader = new StreamReader(responseStream))
							{
								strResult = reader.ReadToEnd();
							}
						}
					}
					throw new Exception(strResult);
				}
				var jResponse = JArray.Parse(json);
				var translation = jResponse.First["translations"].First["text"].ToString();
				return translation;
			}
			catch
			{
				return text;
			}
		}
	}
}
