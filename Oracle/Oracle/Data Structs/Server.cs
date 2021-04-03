using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace Oracle.Data
{
    public class Server
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public List<ulong> GMs { get; set; }
    }
}
