using Sixpence.ORM.Entity;

namespace Sixpence.ORM.Test.Entity
{
    [Table]
    public class UserInfo : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column]
        public string code { get; set; }

        [Column]
        public bool is_admin { get; set; }
    }
}
