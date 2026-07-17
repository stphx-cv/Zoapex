using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages;

public class SalesReportModel(OrderRepository orderRepo, SalesExcelExporter exporter) : PageModel
{
    public SalesReportDto? Report { get; private set; }
    public decimal MaxProductRevenue { get; private set; }

    // El acceso por rol (solo Admin) se aplica con la política "EsAdmin" en Program.cs
    public async Task<IActionResult> OnGetAsync()
    {
        Report = await orderRepo.GetSalesReportAsync();
        MaxProductRevenue = Report.TopProducts.Count > 0
            ? Report.TopProducts.Max(p => p.Revenue)
            : 0m;
        return Page();
    }

    public async Task<IActionResult> OnGetExportAsync()
    {
        var report = await orderRepo.GetSalesReportAsync();
        var bytes = exporter.Build(report);
        var fileName = $"reporte-ventas-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
