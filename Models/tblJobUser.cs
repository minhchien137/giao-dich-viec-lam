using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebGiaoDichViecLam.Models
{
    public class tblJobUser
    {
        [Key]
        public int iJobUserID { get; set; }
        public int iJobID { get; set; }
        public int iProfileID { get; set; }
        public string sType { get; set; }
        public string sMessage { get; set; }
        [ForeignKey("iProfileID")]
        public virtual tblProfileUser TblProfileUser { get; set; } = null!;
        [ForeignKey("iJobID")]
        public virtual tblJob TblJob { get; set; } = null!;

    }
}
