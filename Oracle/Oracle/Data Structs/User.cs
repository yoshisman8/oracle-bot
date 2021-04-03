using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace Oracle.Data
{
    public class User
    {
        [BsonId]
        public ulong ID { get; set; }
        [BsonRef("Actors")]
        public Actor Active {get; set;}
    }
}
