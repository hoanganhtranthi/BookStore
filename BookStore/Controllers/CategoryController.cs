using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Service.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetCategories([FromQuery] PagingRequest pagingRequest, [FromQuery]CategoryRequest categoryRequest)
        {
            var rs = await _categoryService.GetCategories(pagingRequest,categoryRequest);
            return Ok(rs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryResponse>> GetCategory(int id)
        {
            var rs = await _categoryService.GetCategoryByID(id);
            return Ok(rs);
        }

        [HttpPost]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CategoryRequest categoryRequest)
        {
            var rs = await _categoryService.PostCategory(categoryRequest);
            if (rs == null) return BadRequest();
            return Ok(rs);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CategoryResponse>> UpdateCategory([FromBody] CategoryRequest categoryRequest, int id)
        {
            var rs = await _categoryService.PutCategory(id, categoryRequest);
            if (rs == null) return NotFound();
            return Ok(rs);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CategoryResponse>> DeleteCategory(int id)
        {
            var rs = await _categoryService.DeleteCategory(id);
            if (rs == null) return NotFound();
            return Ok(rs);
        }


    }
}
