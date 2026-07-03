using Microsoft.EntityFrameworkCore;
using Zoapex.Web.Data.Models;

namespace Zoapex.Web.Data;

public class CatalogRepository(ZoapexDbContext ctx)
{
    // LINQ: categorías activas para el filtro del catálogo
    public async Task<List<CategoryOptionDto>> GetActiveCategoriesAsync()
    {
        return await ctx.Categories
            .AsNoTracking()
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryOptionDto(c.CategoryId, c.Name))
            .ToListAsync();
    }

    // LINQ: listado del catálogo con nombre de categoría (JOIN implícito)
    public async Task<List<ProductCardDto>> GetCatalogAsync(string? search, int? categoryId)
    {
        var query = ctx.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Status == 1);

        if (categoryId is > 0)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Code.ToLower().Contains(term) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(p => p.Code)
            .Select(p => new ProductCardDto(
                p.ProductId,
                p.Code,
                p.Name,
                p.Description ?? string.Empty,
                p.Price,
                p.Stock,
                p.Category!.Name))
            .ToListAsync();
    }

    // LINQ: detalle de un producto para AJAX (precio y stock al instante)
    public async Task<ProductDetailDto?> GetProductDetailAsync(int productId)
    {
        return await ctx.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.ProductId == productId && p.Status == 1)
            .Select(p => new ProductDetailDto(
                p.ProductId,
                p.Code,
                p.Name,
                p.Price,
                p.Stock,
                p.Category!.Name))
            .FirstOrDefaultAsync();
    }
}
