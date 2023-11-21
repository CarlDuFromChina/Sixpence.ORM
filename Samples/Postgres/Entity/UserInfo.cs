using System;
using System.Runtime.Serialization;
using Sixpence.ORM;
using Sixpence.ORM.Entity;
using Sixpence.ORM.Repository;

namespace Postgres.Entity
{
    [Table]
    public class UserInfo : SormEntity
    {
        [PrimaryColumn]
        public string Id { get; set; }

        [Column]
        public string Code { get; set; }

        [Column]
        public bool IsAdmin { get; set; }
    }
}

