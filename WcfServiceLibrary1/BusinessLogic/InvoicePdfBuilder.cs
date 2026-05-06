using Model;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusinessLogic
{
    /// <summary>
    /// Renders an invoice to a PDF byte array using PdfSharp 1.50
    /// (the .NET Framework 4.7.2-compatible build).
    /// </summary>
    public class InvoicePdfBuilder
    {
        public byte[] Render(Invoice inv, IEnumerable<InvoiceLine> lines, Customer cust)
        {
            using (var doc = new PdfDocument())
            {
                doc.Info.Title  = "Invoice " + inv.InvoiceNumber;
                doc.Info.Author = "B-Managed";

                var page = doc.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                using (var gfx = XGraphics.FromPdfPage(page))
                {
                    var titleFont  = new XFont("Helvetica", 26, XFontStyle.Bold);
                    var headerFont = new XFont("Helvetica", 11, XFontStyle.Bold);
                    var bodyFont   = new XFont("Helvetica", 10, XFontStyle.Regular);
                    var smallFont  = new XFont("Helvetica", 9,  XFontStyle.Regular);

                    double y = 50;

                    gfx.DrawString("INVOICE", titleFont, XBrushes.Black, 40, y);
                    gfx.DrawString(inv.InvoiceNumber, headerFont, XBrushes.Gray, 400, y);
                    gfx.DrawLine(XPens.Black, 40, y + 18, 555, y + 18);
                    y += 50;

                    gfx.DrawString("Bill to", smallFont, XBrushes.Gray, 40, y);
                    gfx.DrawString(cust?.BusinessName ?? "—", headerFont, XBrushes.Black, 40, y + 14);
                    gfx.DrawString(cust?.ContactName ?? "", bodyFont, XBrushes.Black, 40, y + 30);
                    gfx.DrawString(cust?.Email ?? "",        bodyFont, XBrushes.Black, 40, y + 44);
                    gfx.DrawString(cust?.Address ?? "",      bodyFont, XBrushes.Black, 40, y + 58);

                    gfx.DrawString("Issue date", smallFont, XBrushes.Gray, 380, y);
                    gfx.DrawString(inv.IssueDate.ToString("dd MMM yyyy"), bodyFont, XBrushes.Black, 380, y + 14);
                    gfx.DrawString("Due date",   smallFont, XBrushes.Gray, 380, y + 32);
                    gfx.DrawString(inv.DueDate.ToString("dd MMM yyyy"),   bodyFont, XBrushes.Black, 380, y + 46);

                    y += 90;

                    gfx.DrawRectangle(XBrushes.Black, 40, y, 515, 24);
                    gfx.DrawString("Description", headerFont, XBrushes.White, 50, y + 16);
                    gfx.DrawString("Qty",         headerFont, XBrushes.White, 320, y + 16);
                    gfx.DrawString("Unit",        headerFont, XBrushes.White, 380, y + 16);
                    gfx.DrawString("Total",       headerFont, XBrushes.White, 480, y + 16);
                    y += 32;

                    foreach (var l in lines.OrderBy(x => x.Id))
                    {
                        gfx.DrawString(l.Description ?? "",       bodyFont, XBrushes.Black, 50,  y);
                        gfx.DrawString(l.Quantity.ToString("0.##"), bodyFont, XBrushes.Black, 320, y);
                        gfx.DrawString(l.UnitPrice.ToString("N2"),  bodyFont, XBrushes.Black, 380, y);
                        gfx.DrawString(l.LineTotal.ToString("N2"),  bodyFont, XBrushes.Black, 480, y);
                        y += 18;
                    }

                    y += 16;
                    gfx.DrawLine(XPens.LightGray, 320, y, 555, y);
                    y += 12;
                    gfx.DrawString("Subtotal", bodyFont, XBrushes.Black, 380, y);
                    gfx.DrawString(inv.Subtotal.ToString("N2") + " " + inv.Currency, bodyFont, XBrushes.Black, 480, y);
                    y += 16;
                    gfx.DrawString("VAT (" + inv.VatRate.ToString("P0") + ")", bodyFont, XBrushes.Black, 380, y);
                    gfx.DrawString(inv.VatAmount.ToString("N2"), bodyFont, XBrushes.Black, 480, y);
                    y += 22;
                    gfx.DrawRectangle(XBrushes.Black, 360, y - 6, 195, 26);
                    gfx.DrawString("TOTAL", headerFont, XBrushes.White, 380, y + 12);
                    gfx.DrawString(inv.Total.ToString("N2") + " " + inv.Currency, headerFont, XBrushes.White, 470, y + 12);

                    gfx.DrawString("B-Managed - powered by WCF + Razor + WPF",
                        smallFont, XBrushes.Gray, 40, page.Height - 30);
                }

                using (var ms = new MemoryStream())
                {
                    doc.Save(ms, false);
                    return ms.ToArray();
                }
            }
        }
    }
}
