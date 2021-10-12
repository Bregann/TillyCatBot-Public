using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace CatBot
{
    public class Twitter
    {
        private static TwitterClient _appClient;
        public static void SetupTwitter()
        {
            _appClient = new TwitterClient("consumerKey", "consumerSecret", "accessToken", "accessSecret");
            Log.Information("[Twitter] Twitter Setup");
        }

        public static async Task SendTweet(int randomNum)
        {
            try
            {
                //Load the img
                //var catImg = File.ReadAllBytes(@$"\\192.168.1.100\cats\{randomNum}.jpg");
                var catImg = File.ReadAllBytes(@$"catPics/{randomNum}.jpg");
                var uploadedImage = await _appClient.Upload.UploadTweetImageAsync(catImg);

                //Send it
                await _appClient.Tweets.PublishTweetAsync(new PublishTweetParameters($"No. {randomNum} 🐱 🐈 \n \n #Cat #CatPicture")
                {
                    Medias = { uploadedImage }
                });

                await Database.UpdateUsage($"{randomNum}.jpg");
                Log.Information("[Twitter] Tweet sent & database updated");
            }
            catch (Exception e)
            {
                Log.Fatal($"[Twitter] Error sending tweet - {e}");
                return;
            }
        }
    }
}
