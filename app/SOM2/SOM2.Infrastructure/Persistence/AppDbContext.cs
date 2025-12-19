using Microsoft.EntityFrameworkCore;
using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<ManagedHost> ManagedHosts => Set<ManagedHost>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
