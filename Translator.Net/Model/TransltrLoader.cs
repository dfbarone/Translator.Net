using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator.Net.Model
{
    public class TransltrLoader : Loader<TranslationItem>
    {
        public TransltrLoader()
        {

        }

        public TransltrLoader(string text, string fromCulture, string toCulture)
        {
            Init(text, fromCulture, toCulture);
        }

        public void Init(string text, string fromCulture, string toCulture)
        {
            fromCulture = fromCulture.ToLower();
            toCulture = toCulture.ToLower();

            // normalize the culture in case something like en-us was passed 
            // retrieve only en since Google doesn't support sub-locales
            string[] tokens = fromCulture.Split('-');
            FromCulture = tokens.First();

            // normalize ToCulture
            tokens = toCulture.Split('-');
            ToCulture = tokens.First();

            Url = string.Format(@"http://www.transltr.org/api/translate?text={2}&to={1}&from={0}",
                FromCulture,
                ToCulture,
                Uri.EscapeUriString(text));
        }

        override
        public async Task<List<TranslationItem>> Load()
        {
            List<TranslationItem> imageItems = new List<TranslationItem>();

            string content = "";
            try
            {
                content = await GetAsync(Url);
                if (string.IsNullOrEmpty(content))
                    return imageItems;

                JObject obj = JObject.Parse(content);
                string translatedText = (string)obj["translationText"];

                var item = new TranslationItem(translatedText, FromCulture, ToCulture);
                imageItems.Add(item);
                
                return imageItems;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
            }

            return imageItems;
        }
    }
}
