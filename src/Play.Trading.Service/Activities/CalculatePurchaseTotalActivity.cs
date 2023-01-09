using System;
using System.Threading.Tasks;
using Automatonymous;
using MassTransit;
using Play.Common.Contracts.Interfaces;
using Play.Trading.Entities;
using Play.Trading.Service.Contracts;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Activities
{
    public class CalculatePurchaseTotalActivity : IStateMachineActivity<PurchaseState, PurchaseRequested>
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

        public async Task Execute(BehaviorContext<PurchaseState, PurchaseRequested> context, IBehavior<PurchaseState, PurchaseRequested> next)
        {
            var message = context.Message;
            var item = await _catalogItemsRepository.GetAsync(message.ItemId);
            if (item == null)
            {
                throw new UnknownItemException(message.ItemId);
            }
            context.Saga.PurchaseTotal = item.Price * message.Quantity;
            context.Saga.LastUpdatedAt = DateTimeOffset.UtcNow;

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, IBehavior<PurchaseState, PurchaseRequested> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("calculate-purchase-total");
        }
    }
}