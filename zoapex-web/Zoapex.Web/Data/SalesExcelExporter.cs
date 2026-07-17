using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace Zoapex.Web.Data;

// Genera un archivo .xlsx real con EPPlus a partir del reporte de ventas.
public class SalesExcelExporter
{
    private const string Money = "\"S/\" #,##0.00";
    private static readonly System.Drawing.Color Brand = System.Drawing.Color.FromArgb(31, 122, 140);

    public byte[] Build(SalesReportDto data)
    {
        using var package = new ExcelPackage();

        BuildSummarySheet(package, data);
        BuildOrdersSheet(package, data);

        return package.GetAsByteArray();
    }

    private static void BuildSummarySheet(ExcelPackage package, SalesReportDto data)
    {
        var ws = package.Workbook.Worksheets.Add("Resumen");

        ws.Cells["A1"].Value = "Zoapex — Reporte de ventas";
        ws.Cells["A1:D1"].Merge = true;
        StyleTitle(ws.Cells["A1"]);

        ws.Cells["A2"].Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Cells["A2:D2"].Merge = true;

        // KPIs
        var kpis = new (string Label, object Value, bool IsMoney)[]
        {
            ("Pedidos", data.OrderCount, false),
            ("Unidades vendidas", data.UnitsSold, false),
            ("Ticket promedio", data.AverageTicket, true),
            ("Ventas totales", data.TotalRevenue, true),
        };

        var row = 4;
        foreach (var (label, value, isMoney) in kpis)
        {
            ws.Cells[row, 1].Value = label;
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 2].Value = value;
            if (isMoney)
                ws.Cells[row, 2].Style.Numberformat.Format = Money;
            row++;
        }

        // Top productos
        var top = row + 1;
        ws.Cells[top, 1].Value = "Top productos";
        ws.Cells[top, 1, top, 3].Merge = true;
        StyleHeader(ws.Cells[top, 1, top, 3]);

        var head = top + 1;
        ws.Cells[head, 1].Value = "Producto";
        ws.Cells[head, 2].Value = "Cantidad";
        ws.Cells[head, 3].Value = "Ingreso";
        StyleHeader(ws.Cells[head, 1, head, 3]);

        var r = head + 1;
        foreach (var p in data.TopProducts)
        {
            ws.Cells[r, 1].Value = p.ProductName;
            ws.Cells[r, 2].Value = p.Quantity;
            ws.Cells[r, 3].Value = p.Revenue;
            ws.Cells[r, 3].Style.Numberformat.Format = Money;
            r++;
        }

        // Gráfico de barras de top productos (evidencia visual dentro del Excel)
        if (data.TopProducts.Count > 0)
        {
            var chart = ws.Drawings.AddBarChart("TopProductsChart", eBarChartType.BarClustered);
            chart.Title.Text = "Top productos por ingreso";
            chart.SetPosition(head - 1, 0, 4, 0);
            chart.SetSize(460, 260);
            var series = chart.Series.Add(ws.Cells[head + 1, 3, r - 1, 3], ws.Cells[head + 1, 1, r - 1, 1]);
            series.Header = "Ingreso";
            chart.Legend.Remove();
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
    }

    private static void BuildOrdersSheet(ExcelPackage package, SalesReportDto data)
    {
        var ws = package.Workbook.Worksheets.Add("Detalle de pedidos");

        string[] headers = ["Código", "Fecha", "Cliente", "Ítems", "Subtotal", "IGV", "Total"];
        for (var c = 0; c < headers.Length; c++)
            ws.Cells[1, c + 1].Value = headers[c];
        StyleHeader(ws.Cells[1, 1, 1, headers.Length]);

        var row = 2;
        foreach (var o in data.Orders)
        {
            ws.Cells[row, 1].Value = o.Code;
            ws.Cells[row, 2].Value = o.OrderDate;
            ws.Cells[row, 2].Style.Numberformat.Format = "dd/mm/yyyy hh:mm";
            ws.Cells[row, 3].Value = o.CustomerName;
            ws.Cells[row, 4].Value = o.Items;
            ws.Cells[row, 5].Value = o.Subtotal;
            ws.Cells[row, 6].Value = o.Tax;
            ws.Cells[row, 7].Value = o.Total;
            ws.Cells[row, 5, row, 7].Style.Numberformat.Format = Money;
            row++;
        }

        // Fila de totales
        if (data.Orders.Count > 0)
        {
            ws.Cells[row, 1].Value = "TOTAL";
            ws.Cells[row, 1, row, 4].Merge = true;
            ws.Cells[row, 5].Formula = $"SUM(E2:E{row - 1})";
            ws.Cells[row, 6].Formula = $"SUM(F2:F{row - 1})";
            ws.Cells[row, 7].Formula = $"SUM(G2:G{row - 1})";
            ws.Cells[row, 1, row, 7].Style.Font.Bold = true;
            ws.Cells[row, 5, row, 7].Style.Numberformat.Format = Money;
        }

        ws.Cells[1, 1, Math.Max(1, row - 1), headers.Length].AutoFilter = true;
        ws.Cells[ws.Dimension.Address].AutoFitColumns();
    }

    private static void StyleTitle(ExcelRange cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.Size = 16;
        cell.Style.Font.Color.SetColor(Brand);
    }

    private static void StyleHeader(ExcelRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Brand);
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    }
}
