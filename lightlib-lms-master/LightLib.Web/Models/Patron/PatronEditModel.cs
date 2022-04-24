using LightLib.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace LightLib.Web.Models.Patron
{
    public class PatronEditModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Do we want to be able to edit library card id?
        // public int LibraryCardId { get; set; }
        public string Address { get; set; }
        public String DateOfBirth { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string HomeLibrary { get; set; }
        public DbSet<LibraryBranch> LibraryBranches { get; set; }
    }
}