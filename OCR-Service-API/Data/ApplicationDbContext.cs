using Microsoft.EntityFrameworkCore;
using Task_Manager_API.Models;
using Task_Manager_API.Models.Document;

namespace Task_Manager_API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<DocumentModel> Documents { get; set; }
}