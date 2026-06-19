using System.Globalization;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Outbound;

public static class ProductExpiryEmail
{
    public static (string Subject, string Html) Build(IReadOnlyList<Product> products)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var productRows = string.Join("", products.Select(p =>
        {
            var daysLeft = (p.ExpirationDate!.Value.DayNumber - today.DayNumber);
            var expiryStr = p.ExpirationDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return $"""
                <tr>
                  <td style="padding:8px 0;border-bottom:1px solid #eee">{p.Name}</td>
                  <td style="padding:8px 0;border-bottom:1px solid #eee;text-align:center">{expiryStr}</td>
                  <td style="padding:8px 0;border-bottom:1px solid #eee;text-align:right;font-weight:{(daysLeft <= 3 ? "bold;color:#c62828" : "bold;color:#e65100")}">{daysLeft} día{(daysLeft == 1 ? "" : "s")}</td>
                </tr>
                """;
        }));

        var subject = $"VacApp — {products.Count} producto{(products.Count == 1 ? "" : "s")} está{(products.Count == 1 ? "" : "n")} por vencer";

        var html = $"""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1a1a1a">
              <h2 style="color:#e65100;margin-bottom:4px">VacApp · Bovinova</h2>
              <p style="margin-top:0">Los siguientes producto{(products.Count == 1 ? "" : "s")} de tu inventario está{(products.Count == 1 ? "" : "n")} próximos a vencer:</p>
              <table style="width:100%;border-collapse:collapse;margin:16px 0">
                <thead>
                  <tr style="background:#fff3e0">
                    <th style="padding:8px;text-align:left">Producto</th>
                    <th style="padding:8px;text-align:center">Vence</th>
                    <th style="padding:8px;text-align:right">Días</th>
                  </tr>
                </thead>
                <tbody>
                  {productRows}
                </tbody>
              </table>
              <p style="color:#999;font-size:12px">Recordatorio automático de productos próximos a vencer.</p>
            </div>
            """;

        return (subject, html);
    }
}
