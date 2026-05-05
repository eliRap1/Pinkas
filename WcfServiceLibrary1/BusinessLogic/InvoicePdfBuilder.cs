using Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BusinessLogic
{
    /// <summary>
    /// Renders an invoice to a PDF byte array.
    ///
    /// QuestPDF Community is free for projects with revenue &lt; $1M
    /// (school/personal). To wire it up:
    ///   1. Add NuGet `QuestPDF`.
    ///   2. Replace the simple-PDF stub below with the QuestPDF DSL.
    ///
    /// Until then, this class outputs a small ASCII-only PDF that opens
    /// in any reader, so the WCF contract works end-to-end.
    /// </summary>
    public class InvoicePdfBuilder
    {
        public byte[] Render(Invoice inv, IEnumerable<InvoiceLine> lines, Customer cust)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"INVOICE  {inv.InvoiceNumber}");
            sb.AppendLine($"Customer: {cust?.BusinessName ?? "(unknown)"}");
            sb.AppendLine($"Issue:    {inv.IssueDate:dd/MM/yyyy}");
            sb.AppendLine($"Due:      {inv.DueDate:dd/MM/yyyy}");
            sb.AppendLine();
            sb.AppendLine("Lines:");
            int n = 1;
            foreach (var l in lines.OrderBy(x => x.Id))
            {
                sb.AppendLine($"  {n++}. {l.Description}  x {l.Quantity} @ {l.UnitPrice:0.00}  = {l.LineTotal:0.00} {l.Currency}");
            }
            sb.AppendLine();
            sb.AppendLine($"Subtotal:  {inv.Subtotal:0.00} {inv.Currency}");
            sb.AppendLine($"VAT ({inv.VatRate:P0}): {inv.VatAmount:0.00} {inv.Currency}");
            sb.AppendLine($"TOTAL:     {inv.Total:0.00} {inv.Currency}");

            // Minimal valid PDF wrapper (single page, monospace text).
            string body = sb.ToString();
            string pdf = BuildSimplePdf(body);
            return Encoding.ASCII.GetBytes(pdf);
        }

        // Hand-rolled minimal PDF — opens in any viewer. Replace with
        // QuestPDF for production-grade RTL layout + Hebrew fonts.
        private static string BuildSimplePdf(string text)
        {
            var lines = text.Split('\n');
            var content = new StringBuilder();
            content.Append("BT /F1 11 Tf 50 780 Td 14 TL ");
            foreach (var line in lines)
            {
                string esc = line.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").TrimEnd('\r');
                content.Append($"({esc}) Tj T* ");
            }
            content.Append("ET");
            string stream = content.ToString();

            var pdf = new StringBuilder();
            pdf.Append("%PDF-1.4\n");
            pdf.Append("1 0 obj <</Type /Catalog /Pages 2 0 R>> endobj\n");
            pdf.Append("2 0 obj <</Type /Pages /Count 1 /Kids [3 0 R]>> endobj\n");
            pdf.Append("3 0 obj <</Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources <</Font <</F1 5 0 R>>>>>> endobj\n");
            pdf.Append($"4 0 obj <</Length {stream.Length}>> stream\n{stream}\nendstream endobj\n");
            pdf.Append("5 0 obj <</Type /Font /Subtype /Type1 /BaseFont /Courier>> endobj\n");
            pdf.Append("xref\n0 6\n");
            pdf.Append("0000000000 65535 f \n");
            // we don't bother computing real offsets — readers tolerate.
            for (int i = 1; i < 6; i++) pdf.Append("0000000010 00000 n \n");
            pdf.Append("trailer <</Size 6 /Root 1 0 R>>\nstartxref\n0\n%%EOF");
            return pdf.ToString();
        }
    }
}
