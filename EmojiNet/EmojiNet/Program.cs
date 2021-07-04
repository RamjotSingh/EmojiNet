using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EmojiNet
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<int> emojiCollection = GetAllEmojiCodes().ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (int emojiCode in emojiCollection)
            {
                Console.WriteLine(emojiCode);
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (int emojicode in emojiCollection)
            {
                stringBuilder.AppendLine($"                case {emojicode}:");
            }

            stringBuilder.AppendLine("                    return true;");

            List<string> templateLines = File.ReadAllLines("EmojiHelper.cs.template").ToList();

            templateLines[17] = stringBuilder.ToString();

            File.WriteAllLines("UnicodeScalarHelper.cs", templateLines);
        }

        private static async Task<IEnumerable<int>> GetAllEmojiCodes()
        {
            HttpClient httpClient = new HttpClient();

            Console.WriteLine("Getting emoji page from unicode.org. This might take a while.....");
            string emojiPageContents = await httpClient.GetStringAsync("https://unicode.org/emoji/charts/full-emoji-list.html").ConfigureAwait(false);

            IHtmlParser htmlParser = Program.GetHtmlParser();

            Console.WriteLine("Parsing the HTML content");
            IHtmlDocument htmlDocument = htmlParser.ParseDocument(emojiPageContents);

            Console.WriteLine("Collecting all the emoji codes");

            HashSet<int> emojiSet = new HashSet<int>();

            // HTML above contains elements like 
            // <td class='code'><a href='#1f600' name='1f600'>U+1F600</a></td>
            foreach (IElement emojiCodeElement in
                htmlDocument.All.Where(element => element.ClassName == "code" & element.NodeName == "TD" & element.NodeType == NodeType.Element))
            {
                // Get us U+1F600
                string unicodeRepresentation = emojiCodeElement.TextContent;

                // Convert into Hex representation
                unicodeRepresentation = unicodeRepresentation.Replace("U+", "");

                try
                {
                    // Certain emojis are a combination of multiple
                    // For example U+1F636 U+200D U+1F32B U+FE0F
                    foreach (string unicodeEmojiSplit in unicodeRepresentation.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        // Removing 2 invisible characters
                        // U+200D -  Zero Width Joiner
                        // U+FEOF - Variation Selector-16
                        ////if (unicodeEmojiSplit == "200D" || unicodeEmojiSplit == "FE0F")
                        ////{
                        ////    continue;
                        ////}

                        emojiSet.Add(int.Parse(unicodeEmojiSplit, NumberStyles.HexNumber));
                        Console.WriteLine($"{unicodeEmojiSplit} ==> {int.Parse(unicodeEmojiSplit, NumberStyles.HexNumber)}");
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return emojiSet.OrderBy(item => item);
        }

        private static IHtmlParser GetHtmlParser()
        {
            IBrowsingContext browsingContext = BrowsingContext.New(AngleSharp.Configuration.Default);
            return browsingContext.GetService<IHtmlParser>();
        }
    }
}
