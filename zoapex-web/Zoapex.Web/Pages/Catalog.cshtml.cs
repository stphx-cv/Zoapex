using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages;

public class CatalogModel(CatalogRepository catalogRepo) : PageModel
{
    public List<CategoryOptionDto> Categories { get; private set; } = [];
    public string? DbError { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Categories = await catalogRepo.GetActiveCategoriesAsync();
        }
        catch (Exception ex)
        {
            DbError = ex.Message;
        }
    }

    // AJAX: filtra el catálogo sin recargar la página
    public async Task<IActionResult> OnGetSearchAsync(string? q, int? categoryId)
    {
        try
        {
            var products = await catalogRepo.GetCatalogAsync(q, categoryId);
            return new JsonResult(products);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = true, message = ex.Message });
        }
    }

    // AJAX: devuelve precio y stock de un producto al seleccionarlo
    public async Task<IActionResult> OnGetProductAsync(int id)
    {
        try
        {
            var product = await catalogRepo.GetProductDetailAsync(id);
            if (product is null)
                return new JsonResult(new { found = false });

            return new JsonResult(new
            {
                found = true,
                product.ProductId,
                product.Code,
                product.Name,
                product.Price,
                product.Stock,
                product.CategoryName
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { found = false, error = ex.Message });
        }
    }
}
