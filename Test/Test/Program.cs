using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Converter converter = new Converter();
            byte[] output = converter.atob("yhECAXBQBwZzdGF0dXM4yApjc3JmX3Rva2VuIK5VMlhKeDFzb3FCT0otc2gtSlQ2YXF3VnpRazhEMDZKTFpoemdhUndNbnZ4TXlZdzlCdlJTV29Xdk9vWGFHNGZJSDJwSEx0T3lPenIwQlRFdnF3TlZLbWhtVUVVcHFrdEZhSEYyVHBUX3JiODJObmhlVUJjbVVDbi1aVGNyWGlhRkhYajg1dURJV082LUhrYnlETXJNU0tfemQ4MWZzZkpma3oxXzJtaXdycDQlM0QKcHVzaF90b2tlbiCcWnUtT1BJTlZ3cjVrYUYyRnJueXlEYW9kMjlnbmRtay1KM0NXV0J0aEZNTzgwc2preTkxS0hqbU54RWxRV09rLVNONGwtdjMxbkdQR3MxZHRWdTZSdUF3WGlIaGluSDQ5bjRoWDlNRGlHSlNiclFwZ2UyOVI3eWliS0xnd3VJUDdKWlRmU09NMjdrZmZUekxWNU56d1ZRJTNEJTNEBGRhdGFQAQV1c2Vyc1gABmJhbm5lZBAGbG9nZ2VkIBBIRUtSNGxlWDFhQm5HWG8xDXN0YXR1c01lc3NhZ2UgAk9L");
            Parser parser = new Parser(output);
            string result = parser.parse();
            if (result != null)
                File.WriteAllText(@"C:\Users\teap3\Documents\Visual Studio 2015\Projects\Test\vysledek.txt", result); 
            else
                File.WriteAllText(@"C:\Users\teap3\Documents\Visual Studio 2015\Projects\Test\vysledek.txt", "null");


            JObject o;
            JToken token;
            string[] members;
            List<RoomUser> roomUsers = new List<RoomUser>();
            List<Contributions> contributions = new List<Contributions>();
            List<PrivateMessageHistory> privateMsgHist = new List<PrivateMessageHistory>();
            List<ConversationUser> convUsers = new List<ConversationUser>();

            try{o = JObject.Parse(result);}
            catch{return;}


            if ((token = o.SelectToken("$.data.discussion")) != null) //Informace o diskuzi a 
            {
                Discussion discussion = JsonConvert.DeserializeObject<Discussion>(token.ToString());
                token = o.SelectToken("$.data.contributing_users");
                members = token.Select(c => c.ToString()).ToArray(); //Z pole vybere jednotlive prvky a ulozi je do pole stringu
                Console.WriteLine(members.Length);
                foreach (string user in members)
                {
                    roomUsers.Add(JsonConvert.DeserializeObject<RoomUser>(user));
                }
                 
                token = o.SelectToken("$.data.contributions");
                members = token.Select(c => c.ToString()).ToArray();
                foreach (string contribution in members)
                {
                    contributions.Add(JsonConvert.DeserializeObject<Contributions>(contribution));
                }


                 foreach (var User in roomUsers)
                {
                    Console.WriteLine("online: "+User.online);
                    Console.WriteLine("sex_id: "+User.sex_id);
                    Console.WriteLine("nickname: "+User.nickname);
                    Console.WriteLine("id: "+User.id);
                    Console.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\\\");
                    Console.WriteLine("trim: " + User.photo.trim);
                    Console.WriteLine("hash " + User.photo.upload_hash);
                    Console.WriteLine("approvalstatus: "+ User.photo.approval_status);
                    Console.WriteLine("trim_y: "+ User.photo.trim_y);
                    Console.WriteLine("trim_x: "+User.photo.trim_x);
                    Console.WriteLine("trim_w: "+ User.photo.trim_w);
                    Console.WriteLine("secure_hash: " + User.photo.secure_hash);
                    Console.WriteLine("height: "+ User.photo.height);
                    Console.WriteLine("width: "+User.photo.width);
                    Console.WriteLine("like_counter"+User.photo.like_counter);
                    Console.WriteLine("url: " + User.photo.url);
                    Console.WriteLine("trim_h: "+User.photo.trim_h);
                    Console.WriteLine("id: "+User.photo.id);
                    Console.WriteLine("----------------------------------------------------------------");

                }
                foreach (var contribution in contributions)
                 {
                     Console.WriteLine("Create date: " + contribution.create_date);
                     Console.WriteLine("Title: " + contribution.title);
                     Console.WriteLine("Deleted: " + contribution.deleted);
                     Console.WriteLine("Content: " + contribution.content);
                     Console.WriteLine("thread_id: " + contribution.thread_id);
                     Console.WriteLine("parrent_id: " + contribution.parent_id);
                     Console.WriteLine("parrent_user_id: " + contribution.parent_user_id);
                     Console.WriteLine("User ID: " + contribution.user_id);
                     if (contribution.contributions != null)
                     {
                         foreach (var contribution2 in contribution.contributions)
                         {
                             Console.WriteLine("       Create date: " + contribution2.create_date);
                             Console.WriteLine("       Title: " + contribution2.title);
                             Console.WriteLine("       Deleted: " + contribution2.deleted);
                             Console.WriteLine("       Content: " + contribution2.content);
                             Console.WriteLine("       thread_id: " + contribution2.thread_id);
                             Console.WriteLine("       parrent_id: " + contribution2.parent_id);
                             Console.WriteLine("       parrent_user_id: " + contribution2.parent_user_id);
                             Console.WriteLine("       User ID: " + contribution2.user_id);
                             Console.WriteLine("       Contribution: " + contribution2.contributions);
                             Console.WriteLine("       id: " + contribution2.id);
                             Console.WriteLine("       discussion_id :" + contribution2.discussion_id);
                             Console.WriteLine("");
                         }
                     }
                     else
                         Console.WriteLine("Contributions: null");
                     Console.WriteLine("id: " + contribution.id);
                     Console.WriteLine("discussion_id :" + contribution.discussion_id);
                     Console.WriteLine("");
                 }

            }
            else if ((token = o.SelectToken("$.data.last_events")) != null)
            {
                members = token.Select(c => c.ToString()).ToArray();
                foreach (string action in members)
                {
                    o = JObject.Parse(action);
                    if (o.SelectToken("$.event_type").ToString() == "message_sent")
                    {
                        privateMsgHist.Add(JsonConvert.DeserializeObject<PrivateMessageHistory>(action));
                    }
                }
                foreach (var message in privateMsgHist)
                {
                    Console.WriteLine("user id: "+message.user_id);
                    Console.WriteLine("event type: " + message.event_type);
                    Console.WriteLine("created: " + message.created);
                    Console.WriteLine("target user id: " + message.target_user_id);
                    Console.WriteLine("request message: " + message.request_message);
                    Console.WriteLine("message " + message.message);
                    Console.WriteLine("id: " + message.id);
                    Console.WriteLine("-------------------------------------------------");
                }
            }
            else if ((((token = o.SelectToken("$.data.user")) != null) && (token.ToString() != "")) ||  //Vyuziva zkratoveho vyhodnocovani - nedojde k vyhodnoceni druhe podminky, cili ani k prepsani promenne
                     (((token = o.SelectToken("$.data.contact")) != null) && (token.ToString() != "")))
               
            {
                o = JObject.Parse(token.ToString());
                ConversationUser convUser = new ConversationUser();
                convUser.last_action = o.SelectToken("$.last_action").ToString();
                convUser.photo = JsonConvert.DeserializeObject<Photo>(o.SelectToken("$.photo").ToString());
                convUser.nickname = o.SelectToken("$.nickname").ToString();
                convUser.id = o.SelectToken("$.id").ToString();
                convUser.locality = o.SelectToken("$.locality").ToString();
                convUsers.Add(convUser);

                foreach (var user in convUsers)
                {
                    Console.WriteLine("Last action: " + user.last_action);
                    Console.WriteLine("nickname: " + user.nickname);
                    Console.WriteLine("id: " + user.id);
                    Console.WriteLine("locality: " + user.locality);
                    Console.WriteLine("Photo: " + user.photo.url);
                }
            }
            else
            {
                Console.WriteLine("NIC");
            }

            Console.ReadKey();
        }
    }
}
