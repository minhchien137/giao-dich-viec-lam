using System.ComponentModel.DataAnnotations;

namespace WebGiaoDichViecLam.Models
{
    public class tblAccount
    {
        [Key]
        public int iAccountID { get; set; }


        public string sUserName { get; set; } = null!;


        public string sPassword { get; set; } = null!;


        public string sEmailAccount { get; set; } = null!;

        public int iRoleID { get; set; }



    }
}
