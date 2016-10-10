using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using System.Resources;
using System.Collections;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Translator.Net.Model;

namespace ConsoleApplication1
{
    class Program
    {
     
        static void Main(string[] args)
        {
            (new TranslationManager()).Run("");
            Console.ReadLine();
        }
    }
}
