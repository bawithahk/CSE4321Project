using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightLib.Data.Models;
using LightLib.Models;
using LightLib.Models.DTOs;
using LightLib.Models.DTOs.Assets;
using LightLib.Service.Helpers;
using LightLib.Service.Interfaces;
using LightLib.Web.Models.Patron;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LightLib.Web.Controllers {

    public class PatronController : LibraryController
    {
        private readonly IPatronService _patronService;

        public PatronController(IPatronService patronService)
        {
            _patronService = patronService;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int perPage = 10)
        {

            var patrons = await _patronService.GetPaginated(page, perPage);

            if (patrons != null && patrons.Results.Any())
            {
                var viewModel = new PatronIndexModel
                {
                    PageOfPatrons = patrons
                };

                return View(viewModel);
            }

            var emptyModel = new PatronIndexModel
            {
                PageOfPatrons = new PaginationResult<PatronDto>
                {
                    Results = new List<PatronDto>(),
                    PerPage = perPage,
                    PageNumber = page
                }
            };

            return View(emptyModel);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var patron = await _patronService.Get(id);
            var assetsCheckedOut = await _patronService.GetPaginatedCheckouts(patron.Id, 1, 10);
            var checkoutHistory = await _patronService.GetPaginatedCheckoutHistory(patron.Id, 1, 10);
            var holds = await _patronService.GetPaginatedHolds(patron.Id, 1, 10);
            var memberLengthOfTime = TimeSpanHumanizer.GetReadableTimespan(DateTime.UtcNow - patron.CreatedOn);

            var model = new PatronDetailModel()
            {
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
        public async Task<IActionResult> RemoveUser(int id)
        {
            await _patronService.RemovePatron(id);

            int page = 1;
            int perPage = 10;
            var patrons = await _patronService.GetPaginated(page, perPage);

            if (patrons != null && patrons.Results.Any()) {
                var viewModel = new PatronIndexModel {
                    PageOfPatrons = patrons
                };

                return View("Index", viewModel);
            }
            
            var emptyModel = new PatronIndexModel {
                PageOfPatrons = new PaginationResult<PatronDto> {
                    Results = new List<PatronDto>(),
                    PerPage = perPage,
                    PageNumber = page
                }
            };
            
            return View("Index", emptyModel);
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

            var branches = await _patronService.GetLibraryBranches();

            var model = new PatronCreateModel()
            {
                LibraryCards = cardIds,
                LibraryBranches = branches
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPatron(IFormCollection collection)
        {
            var fname = collection["fname"][0];
            var lname = collection["lname"][0];
            var addr = collection["addr"][0];
            var dob = collection["dob"][0];
            var email = collection["email"][0];
            var tel = collection["tel"][0];
            var libraryBranch = collection["libraryBranch"][0];

  

            var libraryCards = await _patronService.GetLibraryCards();
            int[] cardIds = new int[libraryCards.Count()];
            int i = 0;
            foreach (var card in libraryCards)
            {
                cardIds[i] = card.Id;
                i++;
            }
            int id = libraryCards.Count();
            bool found = false;
            while (!found)
            {
                if (cardIds.Contains(id))
                {
                    id++;
                }
                else
                {
                    found = true;
                }
            }

            LibraryCardDto newCard = new LibraryCardDto
            {
                Id = id,
                Created = DateTime.Now
            };
            //var addedCard = await _patronService.AddCard(newCard);

            var branches = await _patronService.GetLibraryBranches();
            LibraryBranch homeBranch = new LibraryBranch { };
            foreach (LibraryBranch branch in branches)
            {
                if (branch.Name == libraryBranch)
                {
                    homeBranch = branch;
                    break;
                }
            }

            LibraryBranchDto homeBranchDto = await _patronService.GetLibraryBranch(homeBranch.Id);

            PatronDto newPatron = new PatronDto
            {
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                FirstName = fname,
                LastName = lname,
                Address = addr,
                DateOfBirth = DateTime.Parse(dob),
                Email = email,
                Telephone = tel,
                LibraryCardId = newCard.Id,
                HomeLibrary = homeBranch.Name,
                //LibraryCard = newCard,
                //HomeLibraryBranch = homeBranchDto,
                //HomeLibraryBranchId = homeBranch.Id
            };

            bool added = await _patronService.Add(newPatron);

            if (added)
            {
                var page = 1;
                var perPage = 10;
                var patrons = await _patronService.GetPaginated(page, perPage);

                if (patrons != null && patrons.Results.Any())
                {
                    var viewModel = new PatronIndexModel
                    {
                        PageOfPatrons = patrons
                    };
                    return View("Index", viewModel);
                }

                var emptyModel = new PatronIndexModel
                {
                    PageOfPatrons = new PaginationResult<PatronDto>
                    {
                        Results = new List<PatronDto>(),
                        PerPage = perPage,
                        PageNumber = page
                    }
                };

                return View("Index", emptyModel);
            }
            else
            {
                var model = new PatronCreateModel()
                {
                    LibraryCards = cardIds,
                    LibraryBranches = branches
                };

                return View("Create", model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var patron = await _patronService.Get(id);

            var branches = await _patronService.GetLibraryBranches();

            var model = new PatronEditModel()
            {
                Id = patron.Id,
                FirstName = patron.FirstName,
                LastName = patron.LastName,
                Email = patron.Email,
                Address = patron.Address,
                DateOfBirth = patron.DateOfBirth.ToString("yyyy-MM-dd"),
                Telephone = patron.Telephone,
                HomeLibrary = patron.HomeLibraryBranch.Name,
                LibraryBranches = branches
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPatron(IFormCollection collection)
        {
            var fname = collection["fname"][0];
            var lname = collection["lname"][0];
            var addr = collection["addr"][0];
            var dob = collection["dob"][0];
            var email = collection["email"][0];
            var tel = collection["tel"][0];
            var libraryBranch = collection["libraryBranch"][0];
            var id = collection["id"][0];

            var libraryCards = await _patronService.GetLibraryCards();
            int[] cardIds = new int[libraryCards.Count()];
            int i = 0;
            foreach (var card in libraryCards)
            {
                cardIds[i] = card.Id;
                i++;
            }
            int libCId = libraryCards.Count();
            bool found = false;
            while (!found)
            {
                if (cardIds.Contains(libCId))
                {
                    libCId++;
                }
                else
                {
                    found = true;
                }
            }

            var branches = await _patronService.GetLibraryBranches();
            LibraryBranch homeBranch = new LibraryBranch { };
            foreach (LibraryBranch branch in branches)
            {
                if (branch.Name == libraryBranch)
                {
                    homeBranch = branch;
                    break;
                }
            }

            LibraryBranchDto homeBranchDto = await _patronService.GetLibraryBranch(homeBranch.Id);

            var patron = await _patronService.Get(int.Parse(id));
            patron.FirstName = fname;
            patron.LastName = lname;
            patron.Address = addr;
            patron.Telephone = tel;
            patron.DateOfBirth = DateTime.Parse(dob);
            patron.Email = email;
            patron.HomeLibrary = homeBranch.Name;
            //patron.HomeLibraryBranch = homeBranchDto;

            bool added = await _patronService.Edit(patron);

            if (added)
            {
                var patronDetail = await _patronService.Get(int.Parse(id));
                var assetsCheckedOut = await _patronService.GetPaginatedCheckouts(patronDetail.Id, 1, 10);
                var checkoutHistory = await _patronService.GetPaginatedCheckoutHistory(patronDetail.Id, 1, 10);
                var holds = await _patronService.GetPaginatedHolds(patronDetail.Id, 1, 10);
                var memberLengthOfTime = TimeSpanHumanizer.GetReadableTimespan(DateTime.UtcNow - patronDetail.CreatedOn);

                var model = new PatronDetailModel()
                {
                    Id = patronDetail.Id,
                    FirstName = patronDetail.FirstName,
                    LastName = patronDetail.LastName,
                    Email = patronDetail.Email,
                    LibraryCardId = patronDetail.LibraryCardId,
                    Address = patronDetail.Address,
                    Telephone = patronDetail.Telephone,
                    HomeLibrary = patronDetail.HomeLibrary,
                    OverdueFees = patronDetail.OverdueFees,
                    AssetsCheckedOut = assetsCheckedOut,
                    CheckoutHistory = checkoutHistory,
                    Holds = holds,
                    HasBeenMemberFor = memberLengthOfTime
                };

                return View("Detail", model);
            }
            else
            {
                var model = new PatronCreateModel()
                {
                    LibraryCards = cardIds,
                    LibraryBranches = branches
                };

                return View("Create", model);
            }
        }
    }
}