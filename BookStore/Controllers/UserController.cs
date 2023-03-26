using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Service.ImplService;
using BookStore.Service.Service.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace BookStore.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetUsers([FromQuery] PagingRequest pagingRequest,[FromQuery]UserRequest userRequest)
        {
            var rs = await _userService.GetUsers(pagingRequest,userRequest);
            return Ok(rs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var rs = await _userService.GetUserByID(id);
            return Ok(rs);
        }

        [HttpGet("getUserByEmail")]
        public async Task<ActionResult<UserResponse>> GetUserByEmail(string email)
        {
            var rs = await _userService.GetUserByEmail(email);
            return Ok(rs);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UserRequest userRequest, int id)
        {
            var rs = await _userService.PutUser(id, userRequest);
            if (rs == null) return NotFound();
            return Ok(rs);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<UserResponse>> DeleteUser(int id)
        {
            var rs = await _userService.DeleteUser(id);
            if (rs == null) return NotFound();
            return Ok(rs);
        }
    }
}
