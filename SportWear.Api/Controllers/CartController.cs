
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
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await GetCartDtoAsync(userId);

            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(AddToCartDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var productSize = await _context.ProductSizes
                .Include(ps => ps.Product)
                .Include(ps => ps.Size)
                .FirstOrDefaultAsync(ps => ps.Id == model.ProductSizeId);

            if (productSize == null)
                return NotFound("Размер товара не найден");

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductSizeId == model.ProductSizeId);

            var currentQuantity = cartItem?.Quantity ?? 0;

            if (currentQuantity + model.Quantity >
                productSize.StockQuantity)
            {
                return BadRequest(
                    $"Недостаточно товара на складе. Доступно: {productSize.StockQuantity - currentQuantity}");
            }

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductSizeId = model.ProductSizeId,
                    Quantity = model.Quantity
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += model.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(await GetCartDtoAsync(userId));
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateCartItem(
            int cartItemId,
            UpdateCartItemDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.ProductSize)
                .FirstOrDefaultAsync(ci =>
                    ci.Id == cartItemId &&
                    ci.Cart.UserId == userId);

            if (cartItem == null)
                return NotFound("Товар в корзине не найден");

            if (model.Quantity >
                cartItem.ProductSize.StockQuantity)
            {
                return BadRequest(
                    $"Недостаточно товара на складе. Доступно: {cartItem.ProductSize.StockQuantity}");
            }

            cartItem.Quantity = model.Quantity;

            await _context.SaveChangesAsync();

            return Ok(await GetCartDtoAsync(userId));
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci =>
                    ci.Id == cartItemId &&
                    ci.Cart.UserId == userId);

            if (cartItem == null)
                return NotFound("Товар в корзине не найден");

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return Ok(await GetCartDtoAsync(userId));
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok("Корзина уже пуста");

            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return Ok("Корзина успешно очищена");
        }

        private async Task<CartDto> GetCartDtoAsync(string userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }

            var cartWithItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductSize)
                        .ThenInclude(ps => ps.Size)
                .FirstAsync();

            var result = new CartDto
            {
                Id = cartWithItems.Id,

                Items = cartWithItems.CartItems.Select(ci =>
                    new CartItemDto
                    {
                        Id = ci.Id,
                        ProductSizeId = ci.ProductSizeId,
                        ProductName = ci.ProductSize.Product.Name,
                        SizeName = ci.ProductSize.Size.Name,
                        UnitPrice = ci.ProductSize.Product.Price,
                        Quantity = ci.Quantity,
                        StockQuantity = ci.ProductSize.StockQuantity,
                        Subtotal = ci.ProductSize.Product.Price * ci.Quantity
                    }).ToList()
            };

            result.TotalAmount = result.Items.Sum(i => i.Subtotal);

            return result;
        }
    }
}
