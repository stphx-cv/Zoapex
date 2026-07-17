using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoapex.Web.Data;

namespace Zoapex.Web.Pages;

public class SalesReportModel(OrderRepository orderRepo, SalesExcelExporter exporter) : PageModel
{
    public SalesReportDto? Report { get; private set; }
    public decimal MaxProductRevenue { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Regla de acceso por rol: solo Administrador ve el reporte
        if (!CustomerSession.IsAdmin(HttpContext.Session))
            return RedirectToPage("/Account/Login", new { returnUrl = "/SalesReport" });

        Report = await orderRepo.GetSalesReportAsync();
        MaxProductRevenue = Report.TopProducts.Count > 0
            ? Report.TopProducts.Max(p => p.Revenue)
            : 0m;
        return Page();
    }

    public async Task<IActionResult> OnGetExportAsync()
    {
        if (!CustomerSession.IsAdmin(HttpContext.Session))
            return RedirectToPage("/Account/Login", new { returnUrl = "/SalesReport" });

        var report = await orderRepo.GetSalesReportAsync();
        var bytes = exporter.Build(report);
        var fileName = $"reporte-ventas-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
