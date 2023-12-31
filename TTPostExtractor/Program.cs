using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TTPostExtractor
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            foreach (var file in args)
            {
                var rawData = File.ReadAllText(file);

                var rawPosts = rawData.Split(Environment.NewLine + Environment.NewLine) ?? throw new Exception();

                var currentPath = string.Join('\\', file.Split('\\')[..^1]);

                foreach (var item in rawPosts)
                {
                    if (item.Contains("Body: ") || item.Contains("Answer: "))
                    {
                        var body = GetBody(item);

                        if (body != null)
                        {
                            var image = GetImageFile(currentPath, item, body);

                            if (image != null)
                            {
                                var imageNumber = GetImageNumber(body);

                                if (!string.IsNullOrEmpty(imageNumber))
                                {
                                    var newPath = Path.Combine(currentPath, $"{imageNumber}{image.Extension}");

                                    if (File.Exists(image.FullName))
                                    {
                                        File.Move(image.FullName, newPath, true);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"File {image.FullName} not found");
                                    }

                                }
                            }
                        }



                    }
                }

            }
        }
        private static string? GetImageNumber(string body)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);
            var numberParagraph = htmlDoc.DocumentNode.SelectNodes("//p").LastOrDefault();

            return numberParagraph == null ? null : NumberGetter().Match(numberParagraph.InnerText).Value;

        }

        private static string? GetBody(string item)
        {
            if (item.Contains("Body: "))
            {
                return item.Substring(item.IndexOf("Body: ") + 6, item.IndexOf("Tags:") - item.IndexOf("Body: ") - 6);

            }
            else if (item.Contains("Answer: "))
            {
                return item.Substring(item.IndexOf("Answer: ") + 8, item.IndexOf("Tags:") - item.IndexOf("Answer: ") - 8);
            }

            return null;
        }

        private static FileInfo? GetImageFile(string currentPath, string post, string body)
        {
            if (post.Contains("Downloaded files: "))
            {
                return new FileInfo(Path.Combine(currentPath, post[(post.IndexOf("Downloaded files: ") + 19)..^1]));
            }
            else
            {
                var bodyHtmlDock = new HtmlDocument();
                bodyHtmlDock.LoadHtml(body);

                try
                {
                    var imagetag = bodyHtmlDock.DocumentNode.SelectNodes("//div/figure/img").LastOrDefault();
                    if (imagetag != null)
                    {
                        var imageLink = imagetag?.Attributes["src"].Value;
                        var imageFileName = imageLink?.Split('/').LastOrDefault();

                        if (imageFileName != null)
                        {
                            return File.Exists(Path.Combine(currentPath, imageFileName))
                                ? new FileInfo(Path.Combine(currentPath, imageFileName))
                                : new FileInfo(Path.Combine(currentPath, imageFileName.Replace(".png", ".pnj.jpg")));
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                    return null;
                }
            }

            return null;
        }

        [GeneratedRegex("<figure\\b[^>]*>(.*?)<\\/figure>", RegexOptions.Multiline | RegexOptions.Compiled)]
        private static partial Regex BodyFigureReplacement();

        [GeneratedRegex("^\\d+", RegexOptions.Compiled)]
        private static partial Regex NumberGetter();
    }
}
