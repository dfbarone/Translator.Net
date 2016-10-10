using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator.Net.Model
{
    public class TranslationItem
    {
        public string FromCulture { get; set; }
        public string ToCulture { get; set; }

        public string Text { get; set; }
        public string TranslatedText { get; set; }

        public TranslationItem(string text, string from, string to)
        {
            Text = text;
            FromCulture = from;
            ToCulture = to;
        }
    }
}
