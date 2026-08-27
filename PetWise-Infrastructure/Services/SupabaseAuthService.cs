
using PetWise.Domain.Entities;
using PetWise_Application.Common.Exceptions;
using PetWise_Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Services
{
    public class SupabaseAuthService : IAuthService
    {
        private readonly Supabase.Client _client;
        private readonly IUserRepository _userRepository;

        public SupabaseAuthService(Supabase.Client client, IUserRepository userRepository)
        {
            _client = client;
            _userRepository = userRepository;
        }

        public async Task<AuthResult?> SignUpAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                var session = await _client.Auth.SignUp(email, password);
                if (session?.User == null) return null;

                return new AuthResult(Guid.Parse(session.User.Id), session.User.Email!, session.AccessToken ?? string.Empty);
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("already registered"))
            {
                throw new ConflictException("An account with this email already exists.");
            }
        }

        public async Task<AuthResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                    return null;

                var userId = Guid.Parse(session.User.Id);
                await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);

                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null) throw new NotFoundException("User profile not found.");

                return new AuthResult(user.Id, user.Email, session.AccessToken);
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("Invalid login credentials"))
            {
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, currentPassword);
                if (session?.User == null) return false;

                await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);

                var updatedUser = await _client.Auth.Update(new Supabase.Gotrue.UserAttributes { Password = newPassword });
                return updatedUser != null;
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("Invalid login credentials"))
            {
                return false;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.Auth.ResetPasswordForEmail(email);
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("too many"))
            {
                throw new TooManyRequestsException("Too many requests. Please wait before trying again.");
            }
            catch
            {
                
            }
        }

        public async Task<AuthResult?> GoogleSignInAsync(string idToken, CancellationToken cancellationToken = default)
        {
            var session = await _client.Auth.SignInWithIdToken(Supabase.Gotrue.Constants.Provider.Google, idToken);
            if (session?.User == null || string.IsNullOrEmpty(session.AccessToken)) return null;

            var userId = Guid.Parse(session.User.Id);
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    Id = userId,
                    Email = session.User.Email!,
                    CreatedAt = DateTime.UtcNow
                };
                await _userRepository.CreateAsync(user, cancellationToken);
            }

            return new AuthResult(user.Id, user.Email, session.AccessToken);
        }
    }
}
