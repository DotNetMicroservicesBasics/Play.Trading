using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common.Contracts.Interfaces;
using Play.Trading.Entities;
using Play.Trading.Service.Contracts.Dtos;

namespace Play.Trading.Service.Controllers
{
    [ApiController]
    [Route("store")]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IRepository<CatalogItem> _catalogItemsRepository;
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;
        private readonly IRepository<ApplicationUser> _usersRepository;

        public StoreController(IRepository<CatalogItem> catalogItemsRepository, IRepository<InventoryItem> inventoryItemsRepository, IRepository<ApplicationUser> usersRepository)
        {
            _catalogItemsRepository = catalogItemsRepository;
            _inventoryItemsRepository = inventoryItemsRepository;
            _usersRepository = usersRepository;
        }

        [HttpGet]
        public async Task<ActionResult<StoreDto>> GetAsync()
        {
            string userId = User.FindFirstValue("sub");

            var catalogItems = await _catalogItemsRepository.GetAllAsync();

            var inventoryItems = await _inventoryItemsRepository.GetAllAsync(item => item.UserId == Guid.Parse(userId));

            var user = await _usersRepository.GetAsync(Guid.Parse(userId));

            var storeDto = new StoreDto(
                catalogItems.Select(catalogItem => new StoreItemDto(
                    catalogItem.Id,
                    catalogItem.Name,
                    catalogItem.Description,
                    catalogItem.Price,
                    inventoryItems.FirstOrDefault(inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0
                )),
                user?.Gil ?? 0
            );

            return Ok(storeDto);
        }


    }
}