using Microsoft.EntityFrameworkCore;
using WebGiaoDichViecLam.Models;

namespace WebGiaoDichViecLam.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Acounct
            modelBuilder.Entity<tblAccount>()
                .HasKey(p => p.iAccountID);
            //Phân loại ngành nghề
            modelBuilder.Entity<tblCategory>()
                .HasKey(p => p.iCategoryID);
            //Công ty
            modelBuilder.Entity<tblCompany>()
                .HasKey(p => p.iCompanyID);
            //Nhà tuyển dụng
            modelBuilder.Entity<tblEmployer>()
                .HasKey(p => p.iEmployerID);
            //Công việc
            modelBuilder.Entity<tblJob>()
                .HasKey(p => p.iJobID);
            //Thông tin người tìm việc
            modelBuilder.Entity<tblProfileUser>()
                .HasKey(p => p.iProfileID);
            //Đánh giá công ty
            modelBuilder.Entity<tblReviews>()
                .HasKey(p => p.iReviewID);
            //Phân quyền
            modelBuilder.Entity<tblRole>()
                .HasKey(p => p.iRoleID);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<tblAccount> tblAccount { get; set; }
        public DbSet<tblCategory> tblCategory { get; set; }
        public DbSet<tblCompany> tblCompany { get; set; }
        public DbSet<tblEmployer> tblEmployer { get; set; }
        public DbSet<tblJob> tblJob { get; set; }
        public DbSet<tblProfileUser> tblProfileUser { get; set; }
        public DbSet<tblReviews> tblReviews { get; set; }
        public DbSet<tblRole> tblRole { get; set; }
        public DbSet<tblJobUser> tblJobUser { get; set; }
        public DbSet<tblSavedJob> tblSavedJob { get; set; }

    }
}
