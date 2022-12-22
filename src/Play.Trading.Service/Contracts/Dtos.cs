using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Play.Trading.Service.Contracts.Dtos
{
    public record SubmitPurchaseDto(
        [Required] Guid? ItemId,
        [Range(1, 100)] int Quantity,
        [Required] Guid? IdempotencyId
    );

    public record PurchaseDto(
        Guid UserId,
        Guid ItemId,
        int Quantity,
        decimal? PurchaseTotal,
        string State,
        string ErrorMessage,
        DateTimeOffset ReceivedAt,
        DateTimeOffset LastUpdatedAt
    );

    public record StoreItemDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int ownedQuantity
    );

    public record StoreDto(
        IEnumerable<StoreItemDto> Items,
        decimal UserGil
    );
}
