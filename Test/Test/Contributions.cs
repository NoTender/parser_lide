using System.Collections.Generic;

namespace Test
{
    public class Contributions
    {
        public int create_date;
        public string title;
        public bool deleted;
        public string content;
        public int thread_id;
        public object parent_id;
        public object parent_user_id;
        public string user_id;
        public List<Contributions> contributions;
        public int id;
        public int discussion_id;
    }
}