using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Models;

namespace Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly VittaDbContext _dbContext;
        public OrdersController(VittaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

       [HttpPost]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromBody] User user)
        {
            if (user == null || user.UserId == 0)
            {
                return BadRequest("Неверные данные пользователя.");
            }
       
            if (_dbContext.Orders == null)
            {
                return NotFound();
            }
       
            var orders = await _dbContext.Orders
                .Where(o => o.UserId == user.UserId).ToListAsync();


            if (orders == null || !orders.Any())
            {
                return NotFound("Заказы для данного пользователя не найдены.");
            }

            var orderDTOs = orders.Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate
            }).ToList();

            return Ok(orderDTOs);
        }

        [HttpPost("create")]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderDto orderDto)
        {
            if (orderDto == null || orderDto.UserId == 0 || !orderDto.OrderItems.Any())
            {
                return BadRequest($"Заказ или товары отсутствуют{orderDto.UserId}");
            }

            var userExists = await _dbContext.Users.AnyAsync(u => u.UserId == orderDto.UserId);
            if (!userExists)
            {
                return BadRequest("Пользователь не найден.");
            }

            var order = new Order
            {
                UserId = orderDto.UserId,
                OrderDate = orderDto.OrderDate,
                TotalAmount = orderDto.TotalAmount,
                OrderItems = orderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                }).ToList()
            };
            _dbContext.Orders.Add(order);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сохранения заказа: {ex.Message}");
            }
            var resultDto = new OrderDto
            {
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    OrderId = oi.OrderId
                }).ToList()
            };
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, resultDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Заказ не найден.");
            }
            return Ok(order);
        }
    }
}
