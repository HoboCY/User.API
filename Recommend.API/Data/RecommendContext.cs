using Microsoft.EntityFrameworkCore;
using Recommend.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Data
{
    public class RecommendContext : DbContext
    {
        public DbSet<ProjectRecommend> ProjectRecommends { get; set; }
        public RecommendContext(DbContextOptions<RecommendContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectRecommend>()
                .ToTable("ProjectRecommends")
                .HasKey(r => r.Id);

            base.OnModelCreating(modelBuilder);

        }
    }
}
