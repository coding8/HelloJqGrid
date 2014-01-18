using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelloJqGrid.Models
{
    public class Guestbook
    {
        [Key]
        public int No{ get; set; }

        [Required]
        public string Message { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        //导航属性
        public Member Members { get; set; }
    }

    [Table("Member")]
    public class Member
    {
        [Key]
        public int No { get; set; }

        [Required(ErrorMessage="姓名必填")]
        [StringLength(5,ErrorMessage="不能超过5个字")]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        public int? Age { get; set; }//“Member”的“Age”属性不能设置为“null”值。必须将该属性设置为类型为“Int32”的非 null 值。-->int? 可空类型

        public DateTime? Birthday { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }

        //导航属性
        public ICollection<Guestbook> Guestbooks { get; set; }

    }

    [Table("Audit")]
    public class Audit
    {
        public long Id { get; set; }
        public string TableName { get; set; }
        public string UserId { get; set; }
        public string Actions { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public Nullable<long> TableIdValue { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    }  
}