using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Trading.Service.Contracts;
using Play.Trading.Service.Contracts.Dtos;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Controllers
{

    [ApiController]
    [Route("purchase")]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly IRequestClient<GetPurchaseState> _purchaseClient;

        public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
        {
            _publishEndpoint = publishEndpoint;
            _purchaseClient = purchaseClient;
        }

        [HttpGet("status/{idempotencyId}")]
        public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid idempotencyId)
        {
            var response = await _purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(idempotencyId));

            var purchaseState = response.Message;

            var purchase = new PurchaseDto(
                purchaseState.UserId,
                purchaseState.ItemId,
                purchaseState.Quantity,
                purchaseState.PurchaseTotal,
                purchaseState.CurrentState,
                purchaseState.ErrorMessage,
                purchaseState.ReceivedAt,
                purchaseState.LastUpdatedAt
            );

            return Ok(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchaseDto)
        {
            var userId = User.FindFirstValue("sub");

            var correlationId = Guid.NewGuid();

            var message = new PurchaseRequested(
                Guid.Parse(userId),
                purchaseDto.ItemId.Value,
                purchaseDto.Quantity,
                purchaseDto.IdempotencyId.Value
            );

            await _publishEndpoint.Publish(message);

            return AcceptedAtAction(nameof(GetStatusAsync), new { purchaseDto.IdempotencyId }, new { purchaseDto.IdempotencyId });
        }
    }
}