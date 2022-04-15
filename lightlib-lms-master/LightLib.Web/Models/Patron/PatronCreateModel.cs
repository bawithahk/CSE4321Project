using LightLib.Data.Models;
using LightLib.Models;
using LightLib.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LightLib.Web.Models.Patron
{
    public class PatronCreateModel
    {
       public int[] LibraryCards { get; set; }

        public DbSet<LibraryBranch> LibraryBranches { get; set; }
    }
}
