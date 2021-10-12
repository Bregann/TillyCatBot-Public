using Reddit;
using Reddit.Exceptions;
using Serilog;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CatBot
{
    public class RedditData
    {
        public static void SetupReddit()
        {
            var reddit = new RedditClient("appID", "refreshToken", "appSecret", "userAgent");
            var user = reddit.User("guineaa");
            var multiReddits = user.Multis();

            var subNames = "";

            foreach (var multiReddit in multiReddits)
            {
                foreach (var subreddit in multiReddit.Subreddits)
                {
                    subNames = subNames + subreddit.Name + "+";
                }
            }

            var subRedditBigList = reddit.Subreddit(subNames);
            subRedditBigList.Posts.GetNew();
            subRedditBigList.Posts.NewUpdated += NewPosts;
            subRedditBigList.Posts.MonitorNew();

            Log.Information($"[Reddit Setup] Subreddits loaded");
        }

        private static async void NewPosts(object sender, Reddit.Controllers.EventArgs.PostsUpdateEventArgs e)
        {
            foreach(var post in e.Added)
            {
                var imgNumber = await Database.GetCatImageRowName();
                var imgName = imgNumber + ".jpg";
                Console.WriteLine(imgName);

                if (post is Reddit.Controllers.SelfPost)
                {
                    Log.Information($"[Reddit Post] Post {post.Title} is a link post - ignoring");
                    continue;
                }

                if (await Database.HasCatBeenDownloaded(post.Permalink))
                {
                    Log.Information($"[Reddit Post] Post {post.Title} is already downloaded - ignoring");
                    continue;
                }

                if (post.NSFW == true && post.Subreddit != "wetpussy")
                {
                    Log.Information($"[Reddit Post] Post {post.Title} is some porn probably lol - downloading to secret folder Kappa");
                    continue;
                }

                if (post.Listing.URL.Contains("reddit.com/gallery") ||
                    post.Listing.URL.Contains("youtube.com") ||
                    post.Listing.URL.Contains("youtu.be") ||
                    post.Listing.URL.EndsWith(".gifv") ||
                    post.Listing.URL.EndsWith("gfycat.com") ||
                    post.Listing.URL.Contains("https://v.redd.it"))
                {
                    Log.Information($"[Reddit Post] Post {post.Title} is not downloadable - ignoring");
                    continue;
                }

                if (post.Listing.URL.Contains("imgur.com"))
                {
                    if (!post.Listing.URL.EndsWith(".png") && !post.Listing.URL.EndsWith(".jpg"))
                    {
                        if (post.Listing.URL.EndsWith("/"))
                        {
                            post.Listing.URL = post.Listing.URL.Remove(post.Listing.URL.Length - 1);
                        }

                        post.Listing.URL = post.Listing.URL + ".png";
                    }
                }

                //Make sure its downloaded successfully
                if (DownloadImage(post.Listing.URL, imgName))
                {
                    await Database.InsertCatImageToDatabase(imgName, post.Permalink, post.Subreddit);
                    Log.Information($"[Reddit Post] Post {post.Title} has successfully downloaded from the subreddit {post.Subreddit} - image name: {imgName}");
                }
            }
        }

        private static bool DownloadImage(string url, string imgName)
        {
            try
            {
                var client = new WebClient();
                var stream = client.OpenRead(url);
                var bitmap = Image.Load(stream);

                if (bitmap != null)
                {
                    bitmap.SaveAsJpeg($@"catPics/{imgName}");
                }

                stream.Flush();
                stream.Close();
                client.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Log.Warning($"[Image Save] Error saving image - {e.Message} // URL {url}");
                return false;
            }
        }
    }
}
