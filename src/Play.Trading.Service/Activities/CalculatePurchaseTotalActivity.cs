using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using Play.Common.Contracts.Interfaces;
using Play.Trading.Entities;
using Play.Trading.Service.Contracts;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Activities
{
    public class CalculatePurchaseTotalActivity : Activity<PurchaseState, PurchaseRequested>
    {

        private readonly IRepository<CatalogItem> _catalogItemsRepository;

        public CalculatePurchaseTotalActivity(IRepository<CatalogItem> catalogItemsRepository)
        {
            _catalogItemsRepository = catalogItemsRepository;
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<PurchaseState, PurchaseRequested> context, Behavior<PurchaseState, PurchaseRequested> next)
        {
            var message = context.Data;
            var item = await _catalogItemsRepository.GetAsync(message.ItemId);
            if (item == null)
            {
                throw new UnknownItemException(message.ItemId);
            }
            context.Instance.PurchaseTotal = item.Price * message.Quantity;
            context.Instance.LastUpdatedAt = DateTimeOffset.UtcNow;

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, Behavior<PurchaseState, PurchaseRequested> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("calculate-purchase-total");
        }
    }
}