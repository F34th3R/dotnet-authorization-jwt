using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Filters;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodoController : ControllerBase
    {
        private readonly ApiDbContext _dbContext;

        public TodoController(ApiDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _dbContext.Items.ToListAsync();
            return Ok(items);
        }

        [HttpPost]
        [IsValid]
        public async Task<IActionResult> CreateItem(ItemData item)
        {
            await _dbContext.Items.AddAsync(item);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetItem", new {item.Id}, item);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _dbContext.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();
            
            return Ok(item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateItem(int id, ItemData item)
        {
            if (id != item.Id) return BadRequest();

            var existItem = await _dbContext.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (existItem == null) return NotFound();

            existItem.Title = item.Title;
            existItem.Description = item.Description;
            existItem.Done = item.Done;

            await _dbContext.SaveChangesAsync();
            
            return CreatedAtAction("GetItem", new {item.Id}, item);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var existItem = await _dbContext.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (existItem == null) return NotFound();

            _dbContext.Items.Remove(existItem);
            await _dbContext.SaveChangesAsync();

            return Ok(existItem);
        }
    }
}
