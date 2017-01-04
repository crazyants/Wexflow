using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using TweetSharp;
using System.Threading;


namespace Wexflow.Tasks.Twitter
{
    public class Twitter:Task
    {
        public string ConsumerKey {get; private set;}
        public string ConsumerSecret { get; private set; }
        public string AccessToken { get; private set; }
        public string AccessTokenSecret { get; private set; }

        public Twitter(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.ConsumerKey = this.GetSetting("consumerKey");
            this.ConsumerSecret = this.GetSetting("consumerSecret");
            this.AccessToken = this.GetSetting("accessToken");
            this.AccessTokenSecret = this.GetSetting("accessTokenSecret");
        }

        public override void Run()
        {
            this.Info("Sending tweets...");

            FileInf[] files = this.SelectFiles();

            if (files.Length > 0)
            {
                TwitterService service = null;
                try
                {
                    service = new TwitterService();
                    service.AuthenticateWith(this.ConsumerKey, this.ConsumerSecret, this.AccessToken, this.AccessTokenSecret);
                    this.Info("Authentication succeeded.");
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("Authentication failed.", e);
                    return;
                }

                foreach (FileInf file in files)
                {
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.Path);
                        foreach (XElement xTweet in xdoc.XPathSelectElements("Tweets/Tweet"))
                        {
                            String status = xTweet.Value;
                            TwitterStatus tweet = service.SendTweet(new SendTweetOptions() { Status = status });
                            this.InfoFormat("Tweet '{0}' sent. id: {1}", status, tweet.Id);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        this.ErrorFormat("An error occured while sending the tweets of the file {0}.", e, file.Path);
                    }
                }
            }

            this.Info("Task finished.");
        }

    }
}

