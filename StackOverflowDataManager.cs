using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace StackMonitor
{

    class StackOverflowDataManager
    {

        public static List<Badge> badgeDefinitions;

        public int GetHighestQuestionId()
        {
            string jsonData = RequestData(@"http://api.stackoverflow.com/1.0/search?key=xyaFp-6X1Umk-YpXHdruDA&tagged=c%23&sort=creation&pagesize=1");
            if (string.IsNullOrEmpty(jsonData))
            {
                return -1;
            }
            return GetHighestQuestionId(jsonData);
        }

        public List<Question> GetNewQuestions(long lastchecktime, string taggroup)
        {
            taggroup = WebUtility.HtmlEncode(taggroup);
            taggroup = taggroup.Replace("#", "%23");
            string jsonData = RequestData(@"http://api.stackexchange.com/2.2/questions?site=stackoverflow&tagged=" + taggroup + @"&sort=creation&body=true&filter=!9YdnSJBlX&fromdate=" + lastchecktime);
            if (string.IsNullOrEmpty(jsonData))
            {
                return null;
            }
            List<Question> result = GetQuestionsNewerThanQuestionId(jsonData);
            result.ForEach(r => r.Owner = PopulateUserInformation(r.Owner.Id, 32)); //todo: we do not have to do this to get the questions now. we can get them from the main response
            return result;
        }
        public List<Reputation> GetReputationChanges(int userid, long lastreputationchecktime)
        {
            string jsonData = RequestData(@"http://api.stackexchange.com/2.2/users/" + userid + @"/reputation?site=stackoverflow&filter=!9YdnS7JvI&fromdate=" + lastreputationchecktime);
            if (string.IsNullOrEmpty(jsonData))
            {
                return null;
            }
            return ParseReputationChanges(jsonData);
        }

        public List<Badge> GetNewBadges(int userid, long lastchecktime)
        {
            string jsonData = RequestData(@"http://api.stackexchange.com/2.2/users/" + userid + @"/badges?site=stackoverflow&pagesize=100&filter=!9YdnSNoYZ&fromdate=" + lastchecktime);
            if (string.IsNullOrEmpty(jsonData))
            {
                return null;
            }
            return ParseNewBadges(jsonData);
        }

        private string RequestData(string url)
        {
            string data = string.Empty;
            using (WebClient client = new WebClient())
            {
                client.Headers["User-Agent"] = "Mozilla/4.0";
                client.Headers["Accept-Encoding"] = "gzip, deflate";
                client.Encoding = Encoding.UTF8;
                try
                {
                    StreamReader reader = new StreamReader(client.OpenRead(url));
                    string responseHeader = client.ResponseHeaders["Content-Encoding"];
                    if (!string.IsNullOrEmpty(responseHeader))
                    {
                        if (responseHeader.ToLower().Contains("gzip"))
                        {
                            byte[] b = DecompressGzip(reader.BaseStream);
                            data = Encoding.GetEncoding(client.Encoding.CodePage).GetString(b);
                        }
                        else
                        {
                            byte[] b = DecompressDeflate(reader.BaseStream);
                            data = Encoding.GetEncoding(client.Encoding.CodePage).GetString(b);
                        }
                    }
                    else
                    {
                        data = reader.ReadToEnd();
                    }
                }
                catch (WebException)
                {
                    //this is probably because of a network issue. The site cannot be reached. In this case, return null to indicate failure.
                    //TODO: Change the notification area icon to reflect communication issues.
                    return null;
                }
            }
            return data;
        }

        private byte[] DecompressDeflate(Stream stream)
        {
            throw new NotImplementedException();
        }

        private byte[] DecompressGzip(Stream inputStream)
        {
            Stream outputStream = new MemoryStream();
            int outputPathLength = 0;
            try
            {
                byte[] readBuffer = new byte[4096];
                using (GZipStream gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    int i = 0;
                    while ((i = gzipStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                    {
                        outputStream.Write(readBuffer, 0, i);
                        outputPathLength += i;
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Some error occured while parsing the inputStream\n\n" + e.ToString());
            }
            byte[] buffer = new byte[outputPathLength];
            outputStream.Position = 0;
            outputStream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private int GetHighestQuestionId(string jsonData)
        {
            var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
            return (int)jobject.SelectToken("questions[0].question_id");
        }

        private List<Question> GetQuestionsNewerThanQuestionId(string jsonData)
        {
            List<Question> results = new List<Question>();
            var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
            var questions = jobject.SelectToken("items");
            foreach (var question in questions)
            {
                Question newquestion = new Question();
                newquestion.Tags.AddRange(question.SelectToken("tags").Select(t => (string)t));
                newquestion.AnswerCount = (int)question.SelectToken("answer_count");
                newquestion.URL = (string)question.SelectToken("link");
                newquestion.Id = (int)question.SelectToken("question_id");
                newquestion.TimeStamp = (long)question.SelectToken("creation_date");
                newquestion.Owner = new User()
                {
                    Id = (int)question.SelectToken("owner.user_id"),
                    Name = (string)question.SelectToken("owner.display_name"),
                    Reputation = (int)question.SelectToken("owner.reputation"),
                };
                newquestion.Score = (int)question.SelectToken("score");
                newquestion.Title = (string)question.SelectToken("title");
                newquestion.Body = ParseHTMLString((string) question.SelectToken("body"));
                results.Add(newquestion);
            }
            return results;
        }

        string ParseHTMLString(string htmlstring)
        {
            return htmlstring.Replace("<p>", "").Replace("</p>", "");
        }

        private List<Reputation> ParseReputationChanges(string jsonData)
        {
            List<Reputation> results = new List<Reputation>();
            var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
            var repchanges = jobject.SelectToken("items");
            foreach (var repchange in repchanges)
            {
                var pointchange = (int)repchange.SelectToken("reputation_change");
                var newreputation = new Reputation();
                newreputation.Type = (string)repchange.SelectToken("post_type");
                newreputation.Title = (string)repchange.SelectToken("title"); 
                newreputation.PositiveRep = pointchange > 0 ? pointchange : 0;
                newreputation.NegativeRep = pointchange < 0 ? pointchange : 0;
                newreputation.TimeStamp = (long)repchange.SelectToken("on_date");
                newreputation.QuestionID = (long)repchange.SelectToken("post_id");
                results.Add(newreputation);
            }
            return results;
        }

        private List<Badge> ParseNewBadges(string jsonData)
        {
            var results = new List<Badge>();
            var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
            var newbadges = jobject.SelectToken("items");
            foreach (var newbadge in newbadges)
            {
                //if (badgeDefinitions == null)
                //{
                //    //the badge definitions is not loaded yet. Load them from the API now.
                //    LoadBadgeDefinitions();
                //}
                Badge badge = new Badge();
                badge.Name = (string)newbadge.SelectToken("name");
                badge.Description = (string)newbadge.SelectToken("description") + '.';
                badge.TimeStamp = (long)newbadge.SelectToken("creation_date");
                //find the rank from the definitions
                //badge.Rank = badgeDefinitions.Where(b => b.Name == badge.Name).Where(b => b.Description == badge.Description).Select(b => b.Rank).Single();
                switch ((string)newbadge.SelectToken("rank"))
                {
                    case "bronze":
                        badge.Rank = BadgeRank.Bronze;
                        break;
                    case "silver":
                        badge.Rank = BadgeRank.Silver;
                        break;
                    case "gold":
                        badge.Rank = BadgeRank.Gold;
                        break;
                }
                results.Add(badge);
            }
            return results;
        }

        //private void LoadBadgeDefinitions()
        //{
        //    string jsonData = RequestData(@"http://api.stackoverflow.com/1.0/badges/");
        //    badgeDefinitions = new List<Badge>();
        //    if (!string.IsNullOrEmpty(jsonData))
        //    {
        //        var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
        //        var definitions = jobject.SelectToken("badges");
        //        foreach (var definition in definitions)
        //        {
        //            Badge badge = new Badge();
        //            badge.Name = (string)definition.SelectToken("name");
        //            //badge.Description = (string)definition.SelectToken("description") + '.'; todo: not given by the new api. have to ask seperately.
        //            switch ((string)definition.SelectToken("rank"))
        //            {
        //                case "bronze":
        //                    badge.Rank = BadgeRank.Bronze;
        //                    break;
        //                case "silver":
        //                    badge.Rank = BadgeRank.Silver;
        //                    break;
        //                case "gold":
        //                    badge.Rank = BadgeRank.Gold;
        //                    break;
        //            }
        //            badgeDefinitions.Add(badge);
        //        }
        //    }
        //}

        public User PopulateUserInformation(int userid, int imagesize)
        {
            User user = new User() { Id = userid };
            string jsonData = RequestData(@"http://api.stackexchange.com/2.2/users/" + userid + "?site=stackoverflow");
            if (string.IsNullOrEmpty(jsonData)) { return null; }
            var jobject = Newtonsoft.Json.Linq.JObject.Parse(jsonData);
            if (jobject.SelectToken("items").Count() == 0) { return null; }
            user.Name = (string)jobject.SelectToken("items[0].display_name");
            user.Reputation = (int)jobject.SelectToken("items[0].reputation");
            user.Badges.Add(new BadgeGroup() { Rank = BadgeRank.Gold, Count = (int)jobject.SelectToken("items[0].badge_counts.gold") });
            user.Badges.Add(new BadgeGroup() { Rank = BadgeRank.Silver, Count = (int)jobject.SelectToken("items[0].badge_counts.silver") });
            user.Badges.Add(new BadgeGroup() { Rank = BadgeRank.Bronze, Count = (int)jobject.SelectToken("items[0].badge_counts.bronze") });
            user.Image = DownloadUserImage((string)jobject.SelectToken("items[0].profile_image"), imagesize);
            return user;
        }


        private Image DownloadUserImage(string imagehash, int imagesize)
        {
            try
            {
                //string imageurl = @"http://www.gravatar.com/avatar/" + imagehash + @"?s=" + imagesize + "&d=identicon&r=PG";
                var imageurl = imagehash;
                using (WebClient client = new WebClient())
                {
                    byte[] data = client.DownloadData(imageurl);
                    return ByteArrayToImage(data);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Image ByteArrayToImage(byte[] byteArray)
        {
            MemoryStream ms = new MemoryStream(byteArray);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
    }

    public class Question
    {
        public List<string> Tags { get; set; }
        public int AnswerCount { get; set; }
        public string URL { get; set; }
        public int Id { get; set; }
        public User Owner { get; set; }
        public int Score { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public long TimeStamp { get; set; }
        public Question()
        {
            Tags = new List<string>();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Reputation { get; set; }
        public string ReputationString
        {
            get
            {
                if (Reputation < 10000)
                    return Reputation.ToString("##,##0");
                return (Reputation / 1000D).ToString("0.0") + " K";
            }
        }
        public List<BadgeGroup> Badges { get; set; }
        public Image Image { get; set; }

        public User()
        {
            Badges = new List<BadgeGroup>();
        }
    }

    public enum BadgeRank { Bronze, Silver, Gold };
    public class BadgeGroup
    {
        public BadgeRank Rank { get; set; }
        public int Count { get; set; }
    }

    public class Badge
    {
        public BadgeRank Rank { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long TimeStamp { get; set; }
    }

    public class Reputation
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int PositiveRep { get; set; }
        public int NegativeRep { get; set; }
        public long TimeStamp { get; set; }
        public long QuestionID { get; set; }
    }


}
