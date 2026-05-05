using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    /// <summary>InvoiceLines table — many lines per invoice (link table).</summary>
    public class InvoiceLineDB : BaseDB
    {
        protected override Base NewEntity() => new InvoiceLine();

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (!(entity is InvoiceLine l)) return;
            try { l.InvoiceId   = Convert.ToInt32(reader["invoiceId"]); }   catch { }
            try { l.Description = reader["description"].ToString(); }       catch { }
            try { l.Quantity    = Convert.ToDouble(reader["quantity"]); }   catch { l.Quantity = 1.0; }
            try { l.UnitPrice   = Convert.ToDecimal(reader["unitPrice"]); } catch { }
            try { l.LineTotal   = Convert.ToDecimal(reader["lineTotal"]); } catch { }
            try { l.Currency    = reader["currency"].ToString(); }          catch { l.Currency = "ILS"; }
        }

        public List<InvoiceLine> GetByInvoice(int invoiceId)
            => Select("SELECT * FROM [InvoiceLines] WHERE [invoiceId] = ? ORDER BY [id]",
                new OleDbParameter("@i", invoiceId)).OfType<InvoiceLine>().ToList();

        public int Insert(InvoiceLine l)
        {
            string sql = @"INSERT INTO [InvoiceLines]
                ([invoiceId],[description],[quantity],[unitPrice],[lineTotal],[currency])
                VALUES (?,?,?,?,?,?)";
            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.Add(new OleDbParameter("@i",   OleDbType.Integer)      { Value = l.InvoiceId });
                cmd.Parameters.Add(new OleDbParameter("@d",   OleDbType.VarWChar, 255){ Value = l.Description ?? "" });
                cmd.Parameters.Add(new OleDbParameter("@q",   OleDbType.Double)       { Value = l.Quantity });
                cmd.Parameters.Add(new OleDbParameter("@up",  OleDbType.Currency)     { Value = l.UnitPrice });
                cmd.Parameters.Add(new OleDbParameter("@lt",  OleDbType.Currency)     { Value = l.LineTotal });
                cmd.Parameters.Add(new OleDbParameter("@cur", OleDbType.VarWChar, 3)  { Value = l.Currency ?? "ILS" });
                conn.Open();
                cmd.ExecuteNonQuery();
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(idCmd.ExecuteScalar());
            }
        }

        public void Delete(int id)
        {
            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand("DELETE FROM [InvoiceLines] WHERE [id]=?", conn))
            {
                cmd.Parameters.Add(new OleDbParameter("@id", OleDbType.Integer) { Value = id });
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
