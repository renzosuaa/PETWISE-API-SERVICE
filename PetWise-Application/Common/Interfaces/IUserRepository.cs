using PetWise.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
