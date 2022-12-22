using System.Threading.Tasks;
using MassTransit;
using Play.Common.Contracts.Interfaces;
using Play.Inventory.Contracts;
using Play.Trading.Entities;

namespace Play.Trading.Service.Consumers
{
    public class InventoryItemUpdatedConsumer : IConsumer<InventoryItemUpdated>
    {
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;

        public InventoryItemUpdatedConsumer(IRepository<InventoryItem> inventoryItemsRepository)
        {
            _inventoryItemsRepository = inventoryItemsRepository;
        }

        public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
        {
            var message = context.Message;

            var inventoryItem = await _inventoryItemsRepository.GetAsync(item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem()
                {
                    UserId = message.UserId,
                    CatalogItemId = message.CatalogItemId,
                    Quantity = message.NewTotalQuantity
                };

                await _inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity = message.NewTotalQuantity;

                await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            }
        }
    }
}