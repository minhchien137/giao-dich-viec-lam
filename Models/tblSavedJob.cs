using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebGiaoDichViecLam.Models
{
    public class tblSavedJob
    {
        [Key]
        public int iJobSaveID { get; set; }
        public int iJobID { get; set; }
        public int iProfileID { get; set; }

        [ForeignKey("iProfileID")]
        public virtual tblProfileUser TblProfileUser { get; set; } = null!;

        [ForeignKey("iJobID")]
        public virtual tblJob TblJob { get; set; } = null!;
    }
}
