using Microsoft.VisualBasic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebGiaoDichViecLam.Models
{
    public class tblJob
    {
        [Key]
        public int iJobID { get; set; }

        public int iCompanyID { get; set; }


        public int iCategoryID { get; set; }


        public string sNameJob { get; set; }

        public string sAddressJob { get; set; }
        public double fSalaryJob { get; set; }


        public string sTypeJob { get; set; }


        public DateTime dPostedDate { get; set; }


        public DateTime dExpiryDate { get; set; }


        public string sDescriptionJob { get; set; }


        [ForeignKey("iCategoryID")]
        public virtual tblCategory TblCategory { get; set; } = null!;
        [ForeignKey("iCompanyID")]
        public virtual tblCompany TblCompany { get; set; } = null!;

    }
}
