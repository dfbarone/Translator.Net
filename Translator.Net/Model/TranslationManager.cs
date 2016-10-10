using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace Translator.Net.Model
{
    class TranslationManager
    {
        DateTime StartTime;
        int PreviousPercent { get; set; }
        int PercentComplete { get; set; }
        int Count { get; set; }
        int Total { get; set; }

        public WebServiceManager _webServiceManager = new WebServiceManager();

        public TranslationManager()
        {
            StartTime = DateTime.Now;
            PreviousPercent = PercentComplete = Count = Total = 0;
        }

        void TextToResx()
        {
            if (File.Exists("Sample\\sample.txt"))
            {
                string sampleTxt = File.ReadAllText("Sample\\sample.txt");
                string[] sampleTxtSentences = sampleTxt.Split('.');

                FileStream resx = new FileStream("Sample\\sample.resx", FileMode.Create, FileAccess.Write);
                ResXResourceWriter rsxw = new ResXResourceWriter(resx);

                int count = 0;
                foreach (string s in sampleTxtSentences)
                {
                    rsxw.AddResource(count.ToString(), s);
                    count++;
                }

                rsxw.Close();
            }
        }

        void XmlToResx(string filein, string fileout)
        {
            // Create a file stream to encapsulate items.resources.
            string n1 = filein + ".xml";
            FileStream fs1 = new FileStream(n1, FileMode.OpenOrCreate, FileAccess.Read);

            // Create a file stream to encapsulate items.resources.
            string n2 = fileout + ".resx";
            FileStream fs2 = new FileStream(n2, FileMode.Create, FileAccess.Write);

            //XmlReader rsxr = XmlReader.Create(fs1);
            ResXResourceWriter rsxw = new ResXResourceWriter(fs2);

            //ResXDataNode node = new ResXDataNode(

            if (File.Exists(n1))
            {
                using (XmlReader rsxr = XmlReader.Create(fs1))
                {
                    while (rsxr.Read())
                    {
                        if (rsxr.IsStartElement())
                        {
                            rsxr.MoveToElement();

                            if (rsxr.HasAttributes)
                            {
                                switch (rsxr.Name)
                                {
                                    case "string":
                                        string name = rsxr.GetAttribute("name");
                                        string content = rsxr.ReadElementContentAsString();
                                        //string key = rsxr.Name;
                                        //string val = rsxr.Value;

                                        Console.WriteLine("key: " + name);
                                        Console.WriteLine("orig: " + content);
                                        //Console.WriteLine("trans: " + tran);

                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            rsxw.AddResource(name, content);
                                        }
                                        break;

                                }
                            }
                        }
                    }
                }
            }

            //rsxr.Close();
            rsxw.Close();
        }

        void NshToResx(string filein, string fileout)
        {
            // Create a file stream to encapsulate items.resources.
            string n1 = filein + ".nsh";
            FileStream fs1 = new FileStream(n1, FileMode.OpenOrCreate, FileAccess.Read);

            // Create a file stream to encapsulate items.resources.
            string n2 = fileout + ".resx";
            FileStream fs2 = new FileStream(n2, FileMode.Create, FileAccess.Write);

            StreamReader rsxr = new StreamReader(fs1);
            ResXResourceWriter rsxw = new ResXResourceWriter(fs2);

            //ResXDataNode node = new ResXDataNode(

            if (File.Exists(n1))
            {
                string line;// = rsxr.;
                while ((line = rsxr.ReadLine()) != null)
                {
                    //string line = rsxr.ReadLine();
                    if (line.Contains("LangString"))
                    {
                        line = line.Replace("LangString", "");
                        line = line.Replace("${LANG_" + "ENGLISH" + "}", "");
                        line = line.Replace("\t", "");
                        char[] a = { '\"' };
                        string[] words = line.Split(a);
                        if (words.Length >= 2)
                        {
                            string key = words[0].Trim(' ');
                            string val = words[1].Trim('\"');

                            rsxw.AddResource(key, val);
                        }
                    }
                }
            }

            rsxr.Close();
            rsxw.Close();

        }


        async void ResxToNsh(string filein, string langKey, string langVal)
        {
            // Create a file stream to encapsulate items.resources.
            //string n1 = filein + "." + langKey + ".resx";
            string n1 = filein + ".resx";
            FileStream fs1 = new FileStream(n1, FileMode.OpenOrCreate, FileAccess.Read);

            // Create a file stream to encapsulate items.resources.
            string n2 = langVal.ToLower() + ".nsh";
            FileStream fs2 = new FileStream(n2, FileMode.Create, FileAccess.Write);

            ResXResourceReader rsxr = new ResXResourceReader(fs1);
            //ResXResourceWriter rsxw = new ResXResourceWriter(fs2);
            StreamWriter rsxw = new StreamWriter(fs2);

            IDictionaryEnumerator ie = rsxr.GetEnumerator();

            rsxw.WriteLine("!ifndef _" + langVal.ToUpper() + "_NSH_");
            rsxw.WriteLine("!define _" + langVal.ToUpper() + "_NSH_");

            while (ie.MoveNext())
            {
                string key = (string)ie.Key;
                //ResXDataNode node = (ResXDataNode)ie.Value;
                string val = (String)ie.Value;

                string tran = "";

                string[] tokens = val.Split('.');
                if (tokens.Length > 1)
                {
                    foreach (string s in tokens)
                    {
                        string l = s;
                        if (!string.IsNullOrEmpty(s))
                            l += ".";
                        var list = await _webServiceManager.Load<TranslationItem>(new TransltrLoader(l, "en", langKey));
                        tran += list.First();
                    }
                }
                else
                {
                    var list = await _webServiceManager.Load<TranslationItem>(new TransltrLoader(val, "en", langKey));
                    tran += list.First();
                }
                string tran2 = tran.Trim('"');

                rsxw.WriteLine("; \"" + val + "\"");
                rsxw.WriteLine("LangString\t" + key + "\t${LANG_" + langVal.ToUpper() + "}\t" + tran);
                rsxw.WriteLine();

                Console.WriteLine("key: " + key);
                Console.WriteLine("orig: " + val);
                Console.WriteLine("trans: " + tran);
            }

            rsxw.WriteLine("!endif");

            rsxr.Close();
            rsxw.Close();
        }

        Task TranslateResx(string filein, string langout)
        {
            return Task.Run(async () =>
            {
                if (!Directory.Exists("Localization\\" + langout))
                    Directory.CreateDirectory("Localization\\" + langout);

                // Create a file stream to encapsulate items.resources.
                FileStream resxFileIn = new FileStream(string.Format("Sample\\sample.resx", filein), 
                    FileMode.OpenOrCreate, 
                    FileAccess.Read);

                // Create a file stream to encapsulate items.resources.
                FileStream resxFileOut = new FileStream(string.Format("Localization\\{0}.{1}.resx", filein, langout), 
                    FileMode.Create, 
                    FileAccess.Write);

                // Create a file stream to encapsulate items.resources.
                FileStream reswFileOut = new FileStream(string.Format("Localization\\{0}\\Resources.resw", langout), 
                    FileMode.Create, 
                    FileAccess.Write);

                ResXResourceReader rsxr = new ResXResourceReader(resxFileIn);
                ResXResourceWriter rsxw = new ResXResourceWriter(resxFileOut);
                ResXResourceWriter rsxw2 = new ResXResourceWriter(reswFileOut);

                await ProcessResx(rsxr, rsxw, rsxw2, langout);
                
                rsxr.Close();
                rsxw.Close();
                rsxw2.Close();
            });
        }

        private Task ProcessResx(ResXResourceReader rsxr, 
            ResXResourceWriter rsxw,
            ResXResourceWriter rsww, 
            string langout)
        {
            return Task.Run( async () =>
            {
                try
                {
                    List<Task> taskList = new List<Task>();
                    IDictionaryEnumerator ie = rsxr.GetEnumerator();
                    TransltrLoader loader = new TransltrLoader();
                    while (ie.MoveNext())
                    {
                        Total++;
                        Task t = TranslateText(loader, rsxw, rsww, (string)ie.Key, (string)ie.Value, langout);
                        taskList.Add(t);    
                    }
                    await Task.WhenAll(taskList.ToArray());
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            });
        }

        private async Task TranslateText(TransltrLoader loader, 
            ResXResourceWriter rsxw, 
            ResXResourceWriter rsww, 
            string key, 
            string text, 
            string langout)
        {
            string tran = "";

            string[] tokens = text.Split('.');
            for (int i = 0; i < tokens.Count(); i++)
            {
                try
                {
                    string s = tokens[i];
                    if (!string.IsNullOrEmpty(s))
                    {
                        loader.Init(s, "en", langout);
                        var list = await _webServiceManager.Load<TranslationItem>(loader);

                        if (list.Count > 0)
                            tran += list.First().Text;
                        else
                            tran += s;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            string tran2 = tran.Trim('"');

            ResXDataNode node = new ResXDataNode(key, tran2);
            node.Comment = text;

            rsxw.AddResource(node);
            rsww.AddResource(node);

            //System.Diagnostics.Debug.WriteLine("key: " + key + " orig: " + text + " trans: " + tran);

            Count++;
        }

        void TryPrint()
        {
            try
            {
                // first time
                int percent = 0;
                if (Count > 0)
                {
                    double d = ((double)Count / Total) * 100;
                    percent = (int)d;
                }
                
                PreviousPercent = PercentComplete;
                PercentComplete = percent;
                Console.Clear();
                    
                Console.WriteLine("Start Time: " + StartTime.ToString("hh:mm:ss"));
                Console.WriteLine("Translating " + " " + percent.ToString() + "% complete");

                TimeSpan ts = DateTime.Now.Subtract(StartTime);
                DateTime dt = new DateTime(ts.Ticks);

                Console.WriteLine("Time elapsed: " + dt.ToString("mm:ss"));
            }
            catch(Exception)
            {

            }
        }

        public void Run(string languagesJsonFile)
        {
            StartTime = DateTime.Now;
            PreviousPercent = PercentComplete = Count = Total = 0;

            Task.Run( async () =>
            {
                languagesJsonFile = "Sample\\languages.json";
                if (File.Exists(languagesJsonFile))
                {
                    string languagesJson = File.ReadAllText(languagesJsonFile);

                    JObject languagesObj = JObject.Parse(languagesJson);
                    JArray languagesArray = (JArray)languagesObj["languages"];

                    if (!Directory.Exists("Localization"))
                        Directory.CreateDirectory("Localization");

                    if (!Directory.Exists("Localization\\" + "en-US"))
                        Directory.CreateDirectory("Localization\\" + "en-US");

                    // Create a file stream to encapsulate items.resources.
                    string n1 = "Localization\\sample.resx";
                    FileStream fs1 = new FileStream(n1, FileMode.OpenOrCreate, FileAccess.Read);
                    byte[] byteStream = new byte[fs1.Length];
                    fs1.Read(byteStream, 0, (int)fs1.Length);

                    string n12 = "Localization\\" + "en-US" + "\\" + "sample.resw";
                    File.Create(n12).Write(byteStream, 0, (int)fs1.Length);

                    //p.NshToResx("sample", "sample");
                    //p.XmlToResx("sample", "sample");

                    List<Task> taskList = new List<Task>();
                    foreach (JObject languageObj in languagesArray)
                    {
                        JObject obj = (JObject)languageObj["language"];
                        string code = (string)obj["code"];
                        Task t = TranslateResx("sample", code);
                        taskList.Add(t);
                    }

                    Timer timer = new Timer(1000);
                    timer.Elapsed += (sender, arg) =>
                    {
                        TryPrint();
                    };
                    timer.Start();

                    await Task.WhenAll(taskList.ToArray());

                    timer.Stop();
                }
            });
        }
    }
}
