using System.Data;
using System.Windows;
using System.Windows.Controls;
using Zoapex.Business;
using Zoapex.Entities;

namespace Zoapex.Presentation.Views;

public partial class CatalogView : Page
{
    private readonly ProductBL  _productBL  = new();
    private readonly CategoryBL _categoryBL = new();

    private DataTable _allProducts = new();
    private bool      _isEditing   = false;
    private int       _editingId   = 0;

    public CatalogView()
    {
        InitializeComponent();
        Loaded += CatalogView_Loaded;
    }

    private void CatalogView_Loaded(object sender, RoutedEventArgs e)
    {
        LoadCategories();
        LoadProducts();
    }

    // ----------------------------------------------------------------
    // Carga de datos
    // ----------------------------------------------------------------

    private void LoadCategories()
    {
        try
        {
            var table = _categoryBL.GetAllCategories();
            cmbCategory.ItemsSource   = table.DefaultView;
            cmbCategory.DisplayMemberPath  = "name";
            cmbCategory.SelectedValuePath  = "category_id";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading categories: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadProducts()
    {
        try
        {
            _allProducts          = _productBL.GetAllProducts();
            dgProducts.ItemsSource = _allProducts.DefaultView;
            lblStatus.Text         = $"{_allProducts.Rows.Count} products";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ----------------------------------------------------------------
    // Filtro en tiempo real (sin llamada a la BD)
    // ----------------------------------------------------------------

    private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        var filter = txtFilter.Text.Trim();
        if (_allProducts.Rows.Count == 0) return;

        _allProducts.DefaultView.RowFilter = string.IsNullOrWhiteSpace(filter)
            ? string.Empty
            : $"name LIKE '%{filter}%' OR code LIKE '%{filter}%' OR category_name LIKE '%{filter}%'";

        lblStatus.Text = $"{_allProducts.DefaultView.Count} of {_allProducts.Rows.Count}";
    }

    // ----------------------------------------------------------------
    // Selección de fila
    // ----------------------------------------------------------------

    private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Nada automático; el usuario pulsa Edit explícitamente
    }

    // ----------------------------------------------------------------
    // Botones CRUD
    // ----------------------------------------------------------------

    private void BtnNew_Click(object sender, RoutedEventArgs e)
    {
        _isEditing      = false;
        _editingId      = 0;
        lblFormTitle.Text = "New Product";
        ClearForm();
        formPanel.IsEnabled = true;
        txtName.Focus();
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (dgProducts.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Select a product to edit.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _isEditing      = true;
        _editingId      = Convert.ToInt32(row["product_id"]);
        lblFormTitle.Text = "Edit Product";
        formPanel.IsEnabled = true;

        txtName.Text        = row["name"].ToString();
        txtDescription.Text = row["description"].ToString();
        txtPrice.Text       = row["price"].ToString();
        txtStock.Text       = row["stock"].ToString();
        txtMinStock.Text    = row["min_stock"].ToString();
        txtImageUrl.Text    = row["image_url"].ToString();

        // Selecciona la categoría en el combo
        cmbCategory.SelectedValue = row["category_id"];
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (dgProducts.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Select a product to delete.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var name = row["name"].ToString();
        var confirm = MessageBox.Show(
            $"Delete product '{name}'?\nThis action cannot be undone.",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            _productBL.DeleteProduct(Convert.ToInt32(row["product_id"]));
            MessageBox.Show("Product deleted successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            LoadProducts();
            ClearForm();
            formPanel.IsEnabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        txtFilter.Text = string.Empty;
        LoadProducts();
    }

    // ----------------------------------------------------------------
    // Guardar (Insertar o Actualizar)
    // ----------------------------------------------------------------

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Construye la entidad desde el formulario
            var product = BuildProductFromForm();

            if (_isEditing)
            {
                product.ProductId = _editingId;
                _productBL.UpdateProduct(product);
                MessageBox.Show("Product updated successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _productBL.InsertProduct(product);
                MessageBox.Show("Product added successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadProducts();
            ClearForm();
            formPanel.IsEnabled = false;
        }
        catch (Exception ex)
        {
            // Muestra el mensaje de validación de la capa Business
            MessageBox.Show(ex.Message, "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        formPanel.IsEnabled = false;
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private ProductEntity BuildProductFromForm()
    {
        if (!decimal.TryParse(txtPrice.Text, out var price))
            throw new Exception("Price must be a valid number.");
        if (!int.TryParse(txtStock.Text, out var stock))
            throw new Exception("Stock must be a valid integer.");
        if (!int.TryParse(txtMinStock.Text, out var minStock))
            throw new Exception("Minimum stock must be a valid integer.");
        if (cmbCategory.SelectedValue == null)
            throw new Exception("A category must be selected.");

        return new ProductEntity
        {
            Name        = txtName.Text.Trim(),
            Description = txtDescription.Text.Trim(),
            Price       = price,
            Stock       = stock,
            MinStock    = minStock,
            CategoryId  = Convert.ToInt32(cmbCategory.SelectedValue),
            ImageUrl    = txtImageUrl.Text.Trim(),
            Status      = 1
        };
    }

    private void ClearForm()
    {
        txtName.Clear();
        txtDescription.Clear();
        txtPrice.Clear();
        txtStock.Clear();
        txtMinStock.Clear();
        txtImageUrl.Clear();
        cmbCategory.SelectedIndex = -1;
    }
}
