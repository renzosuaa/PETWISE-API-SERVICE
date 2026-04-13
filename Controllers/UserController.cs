using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetWise_API.Contracts.User;
using PetWise_API.Models;

namespace PetWise_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Supabase.Client _client;

        public UserController(Supabase.Client client)
        {
            _client = client;
        }


        // CREATING USER IS IN THE AUTH CONTROLLER

        //[HttpPost("/User/")]
        //public async Task<IActionResult> CreateUser(CreateUserRequest request)
        //{
        //    try
        //    {
        //        var user = new User
        //        {
        //            user_id = 
        //            first_name = request.first_name,
        //            last_name = request.last_name,
        //            email = request.email,
        //            password = request.password,
        //            created_at = DateTime.UtcNow
        //        };

        //        var response = await _client.From<User>().Insert(user);

        //        var newUser = response.Models.First();

        //        return Ok(newUser.user_id);

        //    }
        //    catch (Postgrest.Exceptions.PostgrestException ex) when(ex.Message.Contains("duplicate key value"))
        //    {
        //        return Conflict(new { message="Email Already Exist" }); 
        //    }

        //}


        [HttpGet("/User/{user_id}")]
        public async Task<IActionResult> GetUser(string user_id)
        {
            var response = await _client.From<User>()
                                        .Where(u => u.user_id == user_id)
                                        .Get();

            var user = response.Models.FirstOrDefault();
            if (user == null)
                return NotFound();

            
            var dto = new UserResponse
            {
                user_id = user.user_id,
                first_name = user.first_name,
                last_name = user.last_name,
                email = user.email,
                created_at = user.created_at
            };

            return Ok(dto);
        }

        #region PATCH 
        
        #endregion
    }
}
