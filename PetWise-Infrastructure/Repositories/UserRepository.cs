using PetWise.Domain.Entities;
using PetWise_Infrastructure.Persistence.Models;
using PetWise_Application.Common.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Postgrest.Constants;

namespace PetWise_Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly Supabase.Client _client;

        public UserRepository(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var response = await _client.From<UserModel>()
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Get(cancellationToken);

            var model = response.Models.FirstOrDefault();
            if (model == null) return null;

            return MapToDomain(model);
        }

        public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            var model = MapToModel(user);
            await _client.From<UserModel>().Insert(model, cancellationToken: cancellationToken);
        }

        public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var model = MapToModel(user);

            var response = await _client.From<UserModel>()
                .Where(u => u.user_id == user.Id)
                .Update(model, cancellationToken: cancellationToken);

            var updatedModel = response.Models.FirstOrDefault();
            if (updatedModel == null) return null;

            return MapToDomain(updatedModel);
        }

        private static User MapToDomain(UserModel model)
        {
            return new User
            {
                Id = model.user_id,
                Email = model.email ?? string.Empty,
                FirstName = model.first_name,
                LastName = model.last_name,
                ImageUrl = model.image_url,
                Nickname = model.nickname,
                CreatedAt = model.created_at,
                HasCompletedSetup = model.has_completed_setup
            };
        }

        private static UserModel MapToModel(User user)
        {
            return new UserModel
            {
                user_id = user.Id,
                email = user.Email,
                first_name = user.FirstName,
                last_name = user.LastName,
                image_url = user.ImageUrl,
                nickname = user.Nickname,
                created_at = user.CreatedAt,
                has_completed_setup = user.HasCompletedSetup
            };
        }
    }
}