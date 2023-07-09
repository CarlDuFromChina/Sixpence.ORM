using System;
using Sixpence.ORM;
using Sixpence.ORM.Entity;

namespace Postgres.Entity
{
    public class UserInfo : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column]
        public string code { get; set; }

        [Column]
        public bool is_admin { get; set; }

        public static List<UserInfo> FindAll()
        {
            return new Repository<UserInfo>().FindAll().ToList();
        }
    }
}

