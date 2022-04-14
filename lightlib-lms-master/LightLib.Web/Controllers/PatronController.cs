using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightLib.Data.Models;
using LightLib.Models;
using LightLib.Models.DTOs;
using LightLib.Service.Helpers;
using LightLib.Service.Interfaces;
using LightLib.Web.Models.Patron;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LightLib.Web.Controllers {
    
    public class PatronController : LibraryController {
        private readonly IPatronService _patronService;

        public PatronController(IPatronService patronService) {
            _patronService = patronService;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int perPage = 10) {
            
            var patrons = await _patronService.GetPaginated(page, perPage);

            if (patrons != null && patrons.Results.Any()) {
                var viewModel = new PatronIndexModel {
                    PageOfPatrons = patrons
                };

                return View(viewModel);
            }
            
            var emptyModel = new PatronIndexModel {
                PageOfPatrons = new PaginationResult<PatronDto> {
                    Results = new List<PatronDto>(),
                    PerPage = perPage,
                    PageNumber = page
                }
            };
            
            return View(emptyModel);
        }

        public async Task<IActionResult> Detail(int id) {
            var patron = await _patronService.Get(id);
            var assetsCheckedOut = await _patronService.GetPaginatedCheckouts(patron.Id, 1, 10);
            var checkoutHistory = await _patronService.GetPaginatedCheckoutHistory(patron.Id, 1, 10);
            var holds = await _patronService.GetPaginatedHolds(patron.Id, 1, 10);
            var memberLengthOfTime = TimeSpanHumanizer.GetReadableTimespan(DateTime.UtcNow - patron.CreatedOn);

            var model = new PatronDetailModel() {
                Id = patron.Id,
                FirstName = patron.FirstName,
                LastName = patron.LastName,
                Email = patron.Email,
                LibraryCardId = patron.LibraryCardId,
                Address = patron.Address,
                Telephone = patron.Telephone,
                HomeLibrary = patron.HomeLibrary,
                OverdueFees = patron.OverdueFees,
                AssetsCheckedOut = assetsCheckedOut, 
                CheckoutHistory = checkoutHistory,
                Holds = holds,
                HasBeenMemberFor = memberLengthOfTime 
            };

            return View(model);
        }

        // This calls RemovePatron() in LightLib.service/Patrons/PatronService.cs
        // doesn't actually remove the user itself, just passes it the ID of the user
        // doesn't return anything yet. Other functions here return a view.. is this where 
        // we need to return to the Patron Index page or something?
        // TODO: where are we getting the Patron ID from?
        public void RemoveUser(int id)
        {
            _patronService.RemovePatron(id);
        }

        public async Task<IActionResult> Create() {
            
            var libraryCards = await _patronService.GetLibraryCards();
            
            int[] cardIds = new int[libraryCards.Count()];
            int i = 0;
            foreach (var card in libraryCards)
            {
                cardIds[i] = card.Id;
                i++;
            }

            ViewData["cardIds"] = cardIds;

            var branches = await _patronService.GetLibraryBranches();
            ViewData["branches"] = branches;
            


            //var libraryCards = await _patronService.GetLibraryCards();
            var cardIdsStr = "";
            foreach (var card in libraryCards)
            {
                cardIdsStr = cardIdsStr + card.Id + ";";
            }
            ViewData["cardIdsStr"] = cardIdsStr;

            //var branches = await _patronService.GetLibraryBranches();
            var branchNamesStr = "";
            foreach (var branch in branches)
            {
                branchNamesStr = branchNamesStr + branch.Id + "-" + branch.Name + ";";
            }
            ViewData["branchesStr"] = branchNamesStr;
            
            return View();
        }
    }
}