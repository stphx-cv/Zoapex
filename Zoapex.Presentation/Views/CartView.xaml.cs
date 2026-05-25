using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Zoapex.Business;
using Zoapex.Entities;

namespace Zoapex.Presentation.Views;

public partial class CartView : Page
{
    private readonly ProductBL _productBL = new();
    private readonly OrderBL   _orderBL   = new();

    // Carrito: lista observable vinculada al DataGrid de items
    private readonly ObservableCollection<OrderDetailEntity> _cartItems = [];

    // ID de cliente genérico (en el prototipo no hay login)
    private const int GuestCustomerId = 0;

    public CartView()
    {
        InitializeComponent();
        dgCart.ItemsSource = _cartItems;
        _cartItems.CollectionChanged += (_, _) => RefreshTotals();
        Loaded += CartView_Loaded;
    }

    private void CartView_Loaded(object sender, RoutedEventArgs e) => LoadProducts();

    // ----------------------------------------------------------------
    // Carga el catálogo disponible
    // ----------------------------------------------------------------

    private void LoadProducts()
    {
        try
        {
            dgAvailable.ItemsSource = _productBL.GetAllProducts().DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ----------------------------------------------------------------
    // Agregar al carrito
    // ----------------------------------------------------------------

    private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
    {
        if (dgAvailable.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Select a product from the catalog.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(txtQty.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("Quantity must be a positive integer.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var productId   = Convert.ToInt32(row["product_id"]);
        var productName = row["name"].ToString()!;
        var unitPrice   = Convert.ToDecimal(row["price"]);
        var stockAvail  = Convert.ToInt32(row["stock"]);

        // Si el producto ya está en el carrito, incrementa la cantidad
        var existing = _cartItems.FirstOrDefault(c => c.ProductId == productId);
        if (existing != null)
        {
            var newQty = existing.Quantity + qty;
            if (newQty > stockAvail)
            {
                MessageBox.Show(
                    $"Only {stockAvail} units available for '{productName}'.", "Stock",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            existing.Quantity += qty;
            existing.Subtotal  = existing.Quantity * existing.UnitPrice;
            dgCart.Items.Refresh();
        }
        else
        {
            if (qty > stockAvail)
            {
                MessageBox.Show(
                    $"Only {stockAvail} units available for '{productName}'.", "Stock",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _cartItems.Add(new OrderDetailEntity
            {
                ProductId   = productId,
                ProductName = productName,
                UnitPrice   = unitPrice,
                Quantity    = qty,
                Subtotal    = qty * unitPrice
            });
        }

        txtQty.Text = "1";
        RefreshTotals();
    }

    // ----------------------------------------------------------------
    // Eliminar ítem del carrito
    // ----------------------------------------------------------------

    private void BtnRemove_Click(object sender, RoutedEventArgs e)
    {
        if (dgCart.SelectedItem is OrderDetailEntity item)
            _cartItems.Remove(item);
    }

    // ----------------------------------------------------------------
    // Limpiar carrito
    // ----------------------------------------------------------------

    private void BtnClearCart_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count == 0) return;
        var confirm = MessageBox.Show("Clear the entire cart?", "Confirm",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm == MessageBoxResult.Yes)
            _cartItems.Clear();
    }

    // ----------------------------------------------------------------
    // Registrar venta
    // ----------------------------------------------------------------

    private void BtnRegisterSale_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count == 0)
        {
            MessageBox.Show("The cart is empty.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // En el prototipo se usa customer_id = 0 (sin login)
            // Si Supabase exige FK válida, usa NULL o crea un cliente genérico
            var orderId = _orderBL.RegisterOrder(GuestCustomerId, [.. _cartItems]);

            MessageBox.Show($"Sale registered successfully!\nOrder ID: {orderId}",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            _cartItems.Clear();
            LoadProducts(); // Actualiza el stock visible
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ----------------------------------------------------------------
    // Recalcula y muestra los totales
    // ----------------------------------------------------------------

    private void RefreshTotals()
    {
        var items    = _cartItems.ToList();
        var subtotal = OrderBL.CalculateSubtotal(items);
        var tax      = OrderBL.CalculateTax(subtotal);
        var total    = OrderBL.CalculateTotal(subtotal);

        lblSubtotal.Text = $"S/. {subtotal:N2}";
        lblTax.Text      = $"S/. {tax:N2}";
        lblTotal.Text    = $"S/. {total:N2}";
    }
}
