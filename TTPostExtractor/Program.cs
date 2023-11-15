using System.Text.RegularExpressions;
using ReverseMarkdown;

namespace TTPostExtractor
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            var posts = new List<Post>();

            var converterConfig = new Config
            {
                // Include the unknown tag completely in the result (default as well)
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                // will ignore all comments
                RemoveComments = false,
                // remove markdown output for links where appropriate
                SmartHrefHandling = true
            };


            foreach (var file in args)
            {
                var rawData = File.ReadAllText(file);

                var rawPosts = rawData.Split(Environment.NewLine + Environment.NewLine) ?? throw new Exception();

                foreach (var item in rawPosts)
                {
                    if (item != string.Empty)
                    {

                        var id = item.Substring(item.IndexOf("Post id: ") + 9, item.IndexOf("Date:") - item.IndexOf("Post id: ") - 9);

                        var title = string.Empty;
                        if (item.Contains("Title: "))
                        {
                            title = item.Substring(item.IndexOf("Title: ") + 8, item.IndexOf("Body:") - item.IndexOf("Title: ") - 8);
                            if (title == "\n")
                            {
                                title = string.Empty;
                            }
                        }

                        string body;

                        if (item.Contains("Body: "))
                        {
                            body = item.Substring(item.IndexOf("Body: ") + 6, item.IndexOf("Tags:") - item.IndexOf("Body: ") - 6);

                        }
                        else if (item.Contains("Answer: "))
                        {
                            body = item.Substring(item.IndexOf("Answer: ") + 8, item.IndexOf("Tags:") - item.IndexOf("Answer: ") - 8);
                        }
                        else
                        {
                            body = string.Empty;
                        }

                        string tags;
                        if (item.Contains("Downloaded files: "))
                        {
                            tags = item.Substring(item.IndexOf("Tags: "), item.IndexOf("Downloaded files:") - item.IndexOf("Tags: "));
                        }
                        else
                        {
                            tags = item[(item.IndexOf("Tags: ") + 6)..];
                        }

                        if (body != string.Empty)
                        {
                            body = BodyFigureReplacement().Replace(body, string.Empty);
                        }

                        var postBodyMarkdown = new Converter(converterConfig).Convert(body);

                        posts.Add(new Post
                        {
                            Id = id.Replace(Environment.NewLine, string.Empty),
                            Title = title,
                            Body = postBodyMarkdown,
                            Tags = tags,
                        });
                    }
                }

            }

            foreach (var item in posts)
            {
                File.WriteAllText(item.Id + ".md", item.Title + Environment.NewLine + item.Body + Environment.NewLine + item.Tags);
            }
        }

        [GeneratedRegex("<figure\\b[^>]*>(.*?)<\\/figure>", RegexOptions.Multiline | RegexOptions.Compiled)]
        private static partial Regex BodyFigureReplacement();
    }
}
