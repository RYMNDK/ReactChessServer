using Microsoft.EntityFrameworkCore;
using ReactChessServer.Models;

namespace ReactChessServer.Domains;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Room> Rooms { get; set; }
}
