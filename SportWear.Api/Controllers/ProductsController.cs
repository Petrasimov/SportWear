
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SportWear.Api.Data;
using SportWear.Api.Models;
using SportWear.Api.DTOs;

namespace SportWear.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получение каталога с фильтрами
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            string? search,
            int? categoryId,
            int? brandId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue)
            {
                query = query.Where(p =>
                    p.BrandId == brandId.Value);
            }

            var productEntities = await query.ToListAsync();

            var products = productEntities
                .Select(p => MapProduct(p))
                .ToList();

            return Ok(products);
        }

        // Получение одного товара
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Товар не найден");
            }

            return Ok(MapProduct(product));
        }

        // Получение категорий
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Ok(categories);
        }

        // Получение брендов
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            return Ok(brands);
        }

        // Добавление товара
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            ProductCreateDto model)
        {
            if (model.Sizes.Count == 0)
            {
                return BadRequest("Добавьте хотя бы один размер");
            }

            var sizeIds = model.Sizes
                .Select(s => s.SizeId)
                .ToList();

            if (sizeIds.Distinct().Count() != sizeIds.Count)
            {
                return BadRequest("Размеры не должны повторяться");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId);

            var brandExists = await _context.Brands
                .AnyAsync(b => b.Id == model.BrandId);

            var sizesExist = await _context.Sizes
                .CountAsync(s => sizeIds.Contains(s.Id));

            if (!categoryExists || !brandExists ||
                sizesExist != sizeIds.Count)
            {
                return BadRequest(
                    "Указана несуществующая категория, бренд или размер");
            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                BrandId = model.BrandId,

                ProductSizes = model.Sizes.Select(s =>
                    new ProductSize
                    {
                        SizeId = s.SizeId,
                        StockQuantity = s.StockQuantity
                    }).ToList()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Товар успешно добавлен",
                ProductId = product.Id
            });
        }

        // Редактирование товара
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            ProductUpdateDto model)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Товар не найден");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId);

            var brandExists = await _context.Brands
                .AnyAsync(b => b.Id == model.BrandId);

            if (!categoryExists || !brandExists)
            {
                return BadRequest("Категория или бренд не найдены");
            }

            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.ImageUrl = model.ImageUrl;
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;

            await _context.SaveChangesAsync();

            return Ok("Товар успешно обновлён");
        }

        // Изменение остатка конкретного размера
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/sizes/{productSizeId:int}")]
        public async Task<IActionResult> UpdateStock(
            int id,
            int productSizeId,
            UpdateStockDto model)
        {
            var productSize = await _context.ProductSizes
                .FirstOrDefaultAsync(ps =>
                    ps.Id == productSizeId &&
                    ps.ProductId == id);

            if (productSize == null)
            {
                return NotFound("Размер товара не найден");
            }

            productSize.StockQuantity = model.StockQuantity;

            await _context.SaveChangesAsync();

            return Ok("Остаток успешно обновлён");
        }

        // Удаление товара
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Товар не найден");
            }

            var sizeIds = product.ProductSizes
                .Select(ps => ps.Id)
                .ToList();

            var hasOrders = await _context.OrderItems
                .AnyAsync(oi =>
                    sizeIds.Contains(oi.ProductSizeId));

            if (hasOrders)
            {
                return BadRequest(
                    "Нельзя удалить товар, который уже есть в заказах");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Товар успешно удалён");
        }

        // Преобразование модели товара в DTO
        private static ProductDto MapProduct(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ImageUrl = product.ImageUrl,

                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,

                BrandId = product.BrandId,
                BrandName = product.Brand.Name,

                Sizes = product.ProductSizes.Select(ps =>
                    new ProductSizeDto
                    {
                        Id = ps.Id,
                        SizeId = ps.SizeId,
                        SizeName = ps.Size.Name,
                        StockQuantity = ps.StockQuantity
                    }).ToList()
            };
        }
    }
}

