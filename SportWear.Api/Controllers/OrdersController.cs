
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SportWear.Api.Data;
using SportWear.Api.Models;
using SportWear.Api.DTOs;

namespace SportWear.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems.Count == 0)
                return BadRequest("Корзина пуста");

            foreach (var item in cart.CartItems)
            {
                if (item.Quantity > item.ProductSize.StockQuantity)
                {
                    return BadRequest(
                        $"Недостаточно товара на складе: {item.ProductSize.Product.Name}, размер {item.ProductSize.Size.Name}. Доступно: {item.ProductSize.StockQuantity}");
                }
            }


            await using var transaction =
                await _context.Database.BeginTransactionAsync();


            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "New",
                Address = model.Address,
                Phone = model.Phone,
                TotalAmount = cart.CartItems.Sum(item =>
                    item.ProductSize.Product.Price * item.Quantity),

                OrderItems = cart.CartItems.Select(item =>
                    new OrderItem
                    {
                        ProductSizeId = item.ProductSizeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.ProductSize.Product.Price
                    }).ToList()
            };


            foreach (var item in cart.CartItems)
            {
                item.ProductSize.StockQuantity -= item.Quantity;
            }


            _context.Orders.Add(order);


            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();


            await transaction.CommitAsync();

            return Ok(new
            {
                Message = "Заказ успешно оформлен",
                OrderId = order.Id,
                TotalAmount = order.TotalAmount
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Size)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = orders.Select(MapOrder).ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                        .ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Заказ не найден");

            return Ok(MapOrder(order));
        }

        private static OrderDto MapOrder(Order order)
        {
            var result = new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Address = order.Address,
                Phone = order.Phone,
                TotalAmount = order.TotalAmount,

                Items = order.OrderItems.Select(item =>
                    new OrderItemDto
                    {
                        Id = item.Id,
                        ProductName = item.ProductSize.Product.Name,
                        SizeName = item.ProductSize.Size.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.UnitPrice * item.Quantity
                    }).ToList()
            };

            return result;
        }
    }
}
