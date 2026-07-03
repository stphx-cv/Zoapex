using System.Data;
using Zoapex.DataAccess;
using Zoapex.Entities;

namespace Zoapex.Business;

public class ProductBL
{
    private readonly ProductDAL _dal = new();

    public DataTable GetAllProducts() => _dal.GetAllProducts();

    public ProductEntity? GetProduct(int productId) => _dal.GetProduct(productId);

    public void InsertProduct(ProductEntity product)
    {
        Validate(product);
        _dal.InsertProduct(product);
    }

    public void UpdateProduct(ProductEntity product)
    {
        // Verifica que el ID sea válido para una actualización
        if (product.ProductId <= 0)
            throw new Exception("A valid product ID is required.");
        Validate(product);
        _dal.UpdateProduct(product);
    }

    public void DeleteProduct(int productId)
    {
        if (productId <= 0)
            throw new Exception("A valid product ID is required.");
        _dal.DeleteProduct(productId);
    }

    private static void Validate(ProductEntity p)
    {
        // Nombre obligatorio
        if (string.IsNullOrWhiteSpace(p.Name))
            throw new Exception("Product name is required.");

        // Longitud máxima del nombre
        if (p.Name.Length > 80)
            throw new Exception("Product name cannot exceed 80 characters.");

        // Descripción no puede superar 200 caracteres
        if (p.Description.Length > 200)
            throw new Exception("Description cannot exceed 200 characters.");

        // Precio debe ser mayor a cero
        if (p.Price <= 0)
            throw new Exception("Price must be greater than zero.");

        // Stock no puede ser negativo
        if (p.Stock < 0)
            throw new Exception("Stock cannot be negative.");

        // Stock mínimo no puede ser negativo
        if (p.MinStock < 0)
            throw new Exception("Minimum stock cannot be negative.");

        // Categoría obligatoria
        if (p.CategoryId <= 0)
            throw new Exception("A category must be selected.");
    }
}
