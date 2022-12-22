using System.Threading.Tasks;
using MassTransit;
using Play.Common.Contracts.Interfaces;
using Play.Identity.Contracts;
using Play.Trading.Entities;

namespace Play.Trading.Service.Consumers
{
    public class UserUpdatedConsumer : IConsumer<UserUpdated>
    {
        private IRepository<ApplicationUser> _usersRepository;

        public UserUpdatedConsumer(IRepository<ApplicationUser> usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task Consume(ConsumeContext<UserUpdated> context)
        {
            var message = context.Message;

            var user = await _usersRepository.GetAsync(message.UserId);

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    Id = message.UserId,
                    Gil = message.NewTotalGil
                };

                await _usersRepository.CreateAsync(user);
            }
            else
            {
                user.Gil = message.NewTotalGil;

                await _usersRepository.UpdateAsync(user);
            }
        }
    }
}