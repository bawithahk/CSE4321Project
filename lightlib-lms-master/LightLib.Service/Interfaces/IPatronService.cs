using System.Threading.Tasks;
using LightLib.Data.Models;
using LightLib.Models;
using LightLib.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LightLib.Service.Interfaces {
    public interface IPatronService {
        Task<PaginationResult<PatronDto>> GetPaginated(int page, int perPage);
        Task<PaginationResult<CheckoutHistoryDto>> GetPaginatedCheckoutHistory(int patronId, int page, int perPage);
        Task<PaginationResult<HoldDto>> GetPaginatedHolds(int patronId, int page, int perPage);
        Task<PaginationResult<CheckoutDto>> GetPaginatedCheckouts(int patronId, int page, int perPage);
        Task<PatronDto> Get(int patronId);
        Task<bool> Add(PatronDto newPatron);
        Task<DbSet<LibraryCard>> GetLibraryCards();
        Task<DbSet<LibraryBranch>> GetLibraryBranches();
        void RemovePatron(int id);
    }
}