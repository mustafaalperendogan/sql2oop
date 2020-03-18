using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using WindowsFormsToolkit.Controls;

namespace StoredProcesureOlustur
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string _Aciklama = string.Empty;

        private string t = string.Empty;

        public static String GetClrType(string type, string isNullable)
        {
            SqlDbType sqlType = (SqlDbType)Enum.Parse(typeof(SqlDbType), type, true);

            string nullable = isNullable == "YES" ? "?" : "";

            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return "long" + nullable;

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return "byte[]";

                case SqlDbType.Bit:
                    return "bool" + nullable;

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return "string";

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return "DateTime" + nullable;

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return "decimal" + nullable;

                case SqlDbType.Float:
                    return "double" + nullable;

                case SqlDbType.Int:
                    return "int" + nullable;

                case SqlDbType.Real:
                    return "float" + nullable;

                case SqlDbType.UniqueIdentifier:
                    return "Guid" + nullable;

                case SqlDbType.SmallInt:
                    return "short" + nullable;

                case SqlDbType.TinyInt:
                    return "byte" + nullable;

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return "object";

                case SqlDbType.Structured:
                    return "DataTable";

                case SqlDbType.DateTimeOffset:
                    return "DateTimeOffset" + nullable;

                default:
                    return "sqlType";
            }
        }

        private void BaglanVeTablolariGetir()
        {
            dgvTablolar.DataSource = getir("SELECT * FROM INFORMATION_SCHEMA.TABLES ORDER BY table_name");
            dgvTablolar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnBaglanVeGetir_Click(object sender, EventArgs e)
        {
            BaglanVeTablolariGetir();

            f12 = false;
        }

        private string varsayilanConString = string.Empty;

        private void btnInsProcOlustur_Click(object sender, EventArgs e)
        {
            if (dgvKolonlar.Rows.Count <= 0)
            {
                MessageBox.Show("Kolonlar yok!");
            }
            else
            {
                _Aciklama = Microsoft.VisualBasic.Interaction.InputBox("Proc açıklaması", "Açıklama", _Aciklama);
                varsayilanConString = txtConStr.Text.Trim();

                string Aciklama_ = _Aciklama;
                string tabloAdi = string.Empty;

                if (!f12)
                    tabloAdi = dgvTablolar.Rows[dgvTablolar.CurrentRow.Index].Cells["TABLE_NAME"].Value.ToString().Trim();
                else
                    tabloAdi = t;

                nesneOlustur(Aciklama_, tabloAdi);

                procOlustur(Aciklama_, tabloAdi);
                metodOlustur(Aciklama_, tabloAdi);
                selectProcOlustur(Aciklama_, tabloAdi);
                selectMetodOlustur(Aciklama_, tabloAdi);
            }
        }

        private void btnKopyala_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTxtProc.Text);
        }

        private void dgvKolonlar_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        { }

        private void dgvTablolar_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string tabloAdi = dgvTablolar.Rows[dgvTablolar.CurrentRow.Index].Cells["TABLE_NAME"].Value.ToString().Trim();
            TablonunKolonlariniGetir(tabloAdi);
        }

        private bool f12 = false;
        private string secilenProcAdi;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                f12 = true;

                t = Interaction.InputBox("Tablo adını giriniz", "", t);

                TablonunKolonlariniGetir(t);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            splitContainer4.SplitterDistance = 25;
            splitContainer2.SplitterDistance = 37;

            CueTextExtender c = new CueTextExtender();

            c.SetCueText(txtProcBaslangic, "Proc başına eklenecek ifade");
            c.SetCueText(txtConStr, "Conneciton string???");

            txtConStr.Text = Registry.CurrentUser.OpenSubKey("CreateSPbyMAD").GetValue("conStr").ToString().Trim();
        }

        private DataTable getir(string sorgu)
        {
            DataTable dt = new DataTable();

            string connString = txtConStr.Text;
            string query = sorgu;

            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();

            SqlDataAdapter da = new SqlDataAdapter(cmd);

            da.Fill(dt);
            conn.Close();
            da.Dispose();

            return dt;
        }

        private void ınsertİçinSeçiliOlanAlanlarıHepsiİçinSeçToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                bool k = (bool)item.Cells["colSec"].Value;

                item.Cells["colSecNesne"].Value = k;
                item.Cells["colSecSelect"].Value = k;
            }
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTxtProc.Text);
        }

        private void metodOlustur(string aciklama_, string tabloAdi)
        {
            StringBuilder sb = new StringBuilder();

            char kacis = '"';

            if (checkDefaultConString.Checked)
            {
                if (checkDefaultConString.CheckState != CheckState.Indeterminate)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("string conString = " + kacis + varsayilanConString + kacis + ";");
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"///" + aciklama_);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("public int " + tabloAdi + "_Insert (");
            sb.Append("string conStr, ");

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells[0].Value))
                {
                    sb.Append(
                        GetClrType(item.Cells["data_type"].Value.ToString(), item.Cells["is_nullable"].Value.ToString())
                        + " "
                        + item.Cells["column_name"].Value.ToString()
                        + ", ");
                }
            }

            string s = sb.ToString();
            sb.Clear();
            s = s.ToString().Substring(0, s.Length - 2);
            sb.Append(s);

            sb.Append(")");

            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using(SqlConnection cn = new SqlConnection(conStr))");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("SqlCommand sqlcmd = new SqlCommand();");
            sb.Append(Environment.NewLine);
            sb.Append("sqlcmd.Connection = cn;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlcmd.CommandText = " + kacis + txtProcBaslangic.Text + tabloAdi + "_Insert" + kacis + "; ");
            sb.Append(Environment.NewLine);
            sb.Append("sqlcmd.CommandType = CommandType.StoredProcedure;");
            sb.Append(Environment.NewLine);
            sb.Append("SqlParameter sp;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells[0].Value))
                {
                    sb.Append("sp = sqlcmd.Parameters.Add(" + kacis);

                    sb.Append("@" + item.Cells["column_name"].Value);

                    sb.Append(kacis + ", SqlDbType.");

                    sb.Append(item.Cells["data_type"].Value + ");")
                        .Replace(".int", ".Int")
                        .Replace(".datetime", ".DateTime")
                        .Replace(".varchar", ".VarChar")
                        .Replace(".float", ".Float")
                        .Replace(".smallint", ".SmallInt")
                        .Replace(".bit", ".Bit")
                        .Replace(".nvarchar", ".NVarChar")
                        .Replace(".decimal", ".Decimal")
                        .Replace(".nVarChar", ".NVarChar")
                        .Replace(".smallInt", ".SmallInt")
                        .Replace(".uniqueidentifier", ".UniqueIdentifier");

                    sb.Append(Environment.NewLine);

                    sb.Append("sp.Value = ");

                    sb.Append(item.Cells["column_name"].Value + ";");
                    sb.Append(Environment.NewLine);
                }
            }
            sb.Append(Environment.NewLine);
            sb.Append("cn.Open();");
            sb.Append(Environment.NewLine);
            sb.Append(" try");
            sb.Append(Environment.NewLine);
            sb.Append(" {");
            sb.Append(Environment.NewLine);
            sb.Append("    return (int)sqlcmd.ExecuteScalar();");
            sb.Append(Environment.NewLine);
            sb.Append(" }");
            sb.Append(Environment.NewLine);
            sb.Append("catch");
            sb.Append(Environment.NewLine);
            sb.Append(" {");
            sb.Append(Environment.NewLine);
            sb.Append("     return 0;");
            sb.Append(Environment.NewLine);
            sb.Append(" }");
            sb.Append(Environment.NewLine);
            sb.Append("finally");
            sb.Append(Environment.NewLine);
            sb.Append(" {");
            sb.Append(Environment.NewLine);
            sb.Append("     cn.Close();");
            sb.Append(Environment.NewLine);
            sb.Append(" }");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);

            richTxtMetod.Text = sb.ToString();
        }

        private void nesnelerleBirlikteKopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nesne = richTextNesneler.Text;
            string metod = richTextSelectMetod.Text;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(nesne);
            sb.AppendLine(Environment.NewLine);
            sb.Append(metod);

            Clipboard.SetDataObject(sb.ToString());

            checkReadValue.Checked = false;
            checkDefaultConString.Checked = false;
            checkDefaultConString.CheckState = CheckState.Indeterminate;
        }

        private void nesnelerleBirlikteKopyalaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string nesne = richTextNesneler.Text;
            string metod = richTxtMetod.Text;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(nesne);
            sb.AppendLine(Environment.NewLine);
            sb.Append(metod);

            Clipboard.SetDataObject(sb.ToString());

            checkDefaultConString.Checked = false;
            checkDefaultConString.CheckState = CheckState.Indeterminate;
        }

        private void nesneOlustur(string aciklama_, string tabloAdi)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"///" + aciklama_);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("public class " + tabloAdi);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells["colSecNesne"].Value))
                {
                    sb.Append(
                        "    public "
                        +
                        GetClrType(item.Cells["data_type"].Value.ToString(), item.Cells["is_nullable"].Value.ToString())
                        + " "
                        + item.Cells["column_name"].Value.ToString()
                        + "  { get; set; }");
                    sb.Append(Environment.NewLine);
                }
            }

            sb.Append("}");

            richTextNesneler.Text = sb.ToString();
        }

        private void procOlustur(string procAciklama, string tabloAdi)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("-- =============================================");
            sb.Append(Environment.NewLine);
            sb.Append("-- Author: Mustafa Alperen Doğan");
            sb.Append(Environment.NewLine);
            sb.Append("-- Create date: " + DateTime.Now);
            sb.Append(Environment.NewLine);
            sb.Append("-- Modified date: -");
            sb.Append(Environment.NewLine);
            sb.Append("-- Description: " + procAciklama);
            sb.Append(Environment.NewLine);
            sb.Append("-- =============================================");
            sb.Append(Environment.NewLine);
            sb.Append("CREATE PROCEDURE [dbo].[" + txtProcBaslangic.Text + tabloAdi + "_Insert]");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells[0].Value))
                {
                    sb.Append(
                        "@"
                        + item.Cells["column_name"].Value
                        + " "
                        + item.Cells["data_type"].Value
                        + (String.IsNullOrEmpty(item.Cells["CHARACTER_MAXIMUM_LENGTh"].Value.ToString())
                            ? ""
                            :
                            (item.Cells["CHARACTER_MAXIMUM_LENGTh"].Value.ToString().Trim() == "-1"
                            ? "(MAX)"
                            : "(" + item.Cells["CHARACTER_MAXIMUM_LENGTh"].Value + ")"
                            )
                          )
                        + ", ");

                    sb.Append(Environment.NewLine);
                }
            }

            string s = sb.ToString();
            sb.Clear();
            s = s.ToString().Substring(0, s.Length - 4);
            sb.Append(s);

            sb.Append(Environment.NewLine);
            sb.Append("AS");
            sb.Append(Environment.NewLine);
            sb.Append("BEGIN");
            sb.Append(Environment.NewLine);
            sb.Append("INSERT INTO dbo." + tabloAdi + "(");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells[0].Value))
                {
                    sb.Append(item.Cells["column_name"].Value + ", ");
                    sb.Append(Environment.NewLine);
                }
            }

            string s2 = sb.ToString();
            sb.Clear();
            s2 = s2.ToString().Substring(0, s2.Length - 4);
            sb.Append(s2);

            sb.Append(")");
            sb.Append(Environment.NewLine);
            sb.Append("VALUES(");

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells[0].Value))
                {
                    sb.Append("@" + item.Cells["column_name"].Value + ", ");
                    sb.Append(Environment.NewLine);
                }
            }

            string s3 = sb.ToString();
            sb.Clear();
            s3 = s3.ToString().Substring(0, s3.Length - 4);
            sb.Append(s3);

            sb.Append(")");

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("SELECT @@IDENTITY");
            sb.Append(Environment.NewLine);

            sb.Append(Environment.NewLine);
            sb.Append("END");

            richTxtProc.Text = sb.ToString();
        }

        private void ProcOlustur(string p)
        {
            try
            {
                string connString = txtConStr.Text;
                string query = p;

                SqlConnection conn = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                if (cmd.ExecuteNonQuery() == -1)
                {
                    MessageBox.Show("Proc oluşturuldu");
                }
                else
                {
                    MessageBox.Show("Proc oluşturulamadı!!!");
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SecimiTersCevir(string kolonAdi)
        {
            if (dgvKolonlar.Rows.Count > 0)
            {
                foreach (DataGridViewRow item in dgvKolonlar.Rows)
                {
                    item.Cells[kolonAdi].Value = !((bool)item.Cells[kolonAdi].Value);
                }
            }
        }

        private void seçimTersÇevirInsertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SecimiTersCevir("colSec");
        }

        private void seçimTersÇevirNesneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SecimiTersCevir("colSecNesne");
        }

        private void seçimTersÇevirSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SecimiTersCevir("colSecSelect");
        }

        private void selectMetodOlustur(string aciklama_, string tabloAdi)
        {
            char kacis = '"';

            StringBuilder sb = new StringBuilder();

            if (checkDefaultConString.Checked)
            {
                if (checkDefaultConString.CheckState != CheckState.Indeterminate)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("string conString = " + kacis + varsayilanConString + kacis + ";");
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            if (checkReadValue.Checked)
            {
                sb.Append(Environment.NewLine);
                sb.Append("private static object ReadValue(DataRow item, string name, string nullable)");
                sb.Append(Environment.NewLine);
                sb.Append("{");
                sb.Append(Environment.NewLine);
                sb.Append("    if (nullable == " + kacis + "YES" + kacis + ")");
                sb.Append(Environment.NewLine);
                sb.Append("        return ((item[name] == DBNull.Value) ? string.Empty : item[name]);");
                sb.Append(Environment.NewLine);
                sb.Append("    else");
                sb.Append(Environment.NewLine);
                sb.Append("        return item[name];");
                sb.Append(Environment.NewLine);
                sb.Append("}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            sb.Append("#region " + tabloAdi + "_Select");
            sb.Append(Environment.NewLine);
            sb.Append("private List<" + tabloAdi + "> _" + tabloAdi.ToLowerInvariant() + " = new List<" + tabloAdi + ">();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"///" + aciklama_);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("/// <param name=" + kacis + "where" + kacis + ">Where ifadesi tam olarak yazılmaldır. (Ör: where DOCODE = 'ab12')</param>");
            sb.Append(Environment.NewLine);
            sb.Append("public void " + tabloAdi + "_Select(");

            sb.Append("string conStr, ");

            sb.Append("string where = " + kacis + "where 1=1" + kacis + ")");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using (DataTable dataTable = new DataTable())");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using(SqlConnection cn = new SqlConnection(conStr))");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("SqlCommand sqlCommand = new SqlCommand();");
            sb.Append(Environment.NewLine);
            sb.Append("SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.Connection = cn;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandType = CommandType.StoredProcedure;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandText = " + kacis + txtProcBaslangic.Text + tabloAdi + "_Select" + kacis + "; ");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.Parameters.Add(" + kacis + "@where" + kacis + ", SqlDbType.NVarChar);");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.Parameters[" + kacis + "@where" + kacis + "].Value = where; ");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("cn.Open();");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.SelectCommand = sqlCommand;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.Fill(dataTable);");
            sb.Append(Environment.NewLine);
            sb.Append("cn.Close();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("_" + tabloAdi.ToLowerInvariant() + ".Clear();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("foreach (DataRow item in dataTable.Rows)");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("_" + tabloAdi.ToLowerInvariant() + ".Add(new " + tabloAdi);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells["colSecSelect"].Value))
                {
                    sb.Append(
                        item.Cells["column_name"].Value.ToString()
                        + "=("
                        + GetClrType(item.Cells["data_type"].Value.ToString(), item.Cells["is_nullable"].Value.ToString())
                        + ")"
                        + "ReadValue(item,"
                        + kacis
                        + item.Cells["column_name"].Value.ToString()
                        + kacis
                        + ", "
                        + kacis
                        + item.Cells["is_nullable"].Value.ToString().ToUpper()
                        + kacis
                        + "),"
                        );

                    sb.Append(Environment.NewLine);
                }
            }

            string s = sb.ToString();
            sb.Clear();
            s = s.ToString().Substring(0, s.Length - 3);
            sb.Append(s);

            sb.Append(Environment.NewLine);
            sb.Append("});");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("#endregion");

            richTextSelectMetod.Text = sb.ToString();
        }

        private void selectProcOlustur(string procAciklama, string tabloAdi)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbKolonlar = new StringBuilder();

            sb.Append("-- =============================================");
            sb.Append(Environment.NewLine);
            sb.Append("-- Author: Mustafa Alperen Doğan");
            sb.Append(Environment.NewLine);
            sb.Append("-- Create date: " + DateTime.Now);
            sb.Append(Environment.NewLine);
            sb.Append("-- Modified date: -");
            sb.Append(Environment.NewLine);
            sb.Append("-- Description: " + procAciklama);
            sb.Append(Environment.NewLine);
            sb.Append("-- =============================================");
            sb.Append(Environment.NewLine);
            sb.Append("CREATE PROCEDURE [dbo].[" + txtProcBaslangic.Text + tabloAdi + "_Select]");
            sb.Append(Environment.NewLine);
            sb.Append("@where NVARCHAR(250)");

            foreach (DataGridViewRow item in dgvKolonlar.Rows)
            {
                if (Convert.ToBoolean(item.Cells["colSecSelect"].Value))
                {
                    sbKolonlar.Append(item.Cells["column_name"].Value + ", ");
                }
            }

            string s = sbKolonlar.ToString();
            sbKolonlar.Clear();
            s = s.ToString().Substring(0, s.Length - 2);
            sbKolonlar.Append(s);

            sb.Append(Environment.NewLine);
            sb.Append("AS");
            sb.Append(Environment.NewLine);
            sb.Append("BEGIN");
            sb.Append(Environment.NewLine);
            sb.Append("    DECLARE @s NVARCHAR(MAX) =N'SELECT " + sbKolonlar.ToString() + " FROM dbo." + tabloAdi + " ' + @where");
            sb.Append(Environment.NewLine);
            sb.Append("    EXEC sp_executesql @s");
            sb.Append(Environment.NewLine);
            sb.Append("END");

            richTextProcSelect.Text = sb.ToString();
        }

        private void sQLdeOluşturToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcOlustur(richTxtProc.Text.Trim());
        }

        private void TablonunKolonlariniGetir(string tabloAdi)
        {
            _Aciklama = string.Empty;

            if (MessageBox.Show(tabloAdi + " tablosu için kolonları getirmek istiyormusunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dgvKolonlar.DataSource = getir("SELECT column_name, is_nullable, data_type, CHARACTER_MAXIMUM_LENGTh FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tabloAdi + "' ORDER BY ORDINAL_POSITION");
                dgvKolonlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                foreach (DataGridViewRow item in dgvKolonlar.Rows)
                {
                    item.Cells["colSec"].Value = true;
                    item.Cells["colSecSelect"].Value = true;
                    item.Cells["colSecNesne"].Value = true;
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTxtMetod.Text);
            checkDefaultConString.Checked = false;
            checkDefaultConString.CheckState = CheckState.Indeterminate;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTextNesneler.Text);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTextProcSelect.Text);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ProcOlustur(richTextProcSelect.Text.Trim());
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTextSelectMetod.Text);

            checkReadValue.Checked = false;
            checkDefaultConString.Checked = false;
            checkDefaultConString.CheckState = CheckState.Indeterminate;
        }

        private void txtConStr_TextChanged(object sender, EventArgs e)
        {
            Registry.CurrentUser.CreateSubKey("CreateSPbyMAD").SetValue("conStr", txtConStr.Text.Trim());
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            splitContainer4.SplitterDistance = 25;
            splitContainer2.SplitterDistance = 37;
        }

        private void btnBaglanVeProcedureGetir_Click(object sender, EventArgs e)
        {
            dgvProcedureler.DataSource = getir("SELECT o.name,o.xtype,o.crdate FROM sysobjects o JOIN syscomments c ON o.id=c.id WHERE o.xtype='P' ORDER BY o.name");
            dgvProcedureler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void dgvProcedureler_DoubleClick(object sender, EventArgs e)
        {
            secilenProcAdi = dgvProcedureler.Rows[dgvProcedureler.CurrentCell.RowIndex].Cells["name"].Value.ToString();

            dgvProcedureParametreleri.DataSource = getir("select PARAMETER_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.PARAMETERS WHERE specific_name = '" + secilenProcAdi + "' AND PARAMETER_MODE = 'IN'");

            dgvProcedureParametreleri.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void btnProcedureCalistir_Click(object sender, EventArgs e)
        {
            _Aciklama = Microsoft.VisualBasic.Interaction.InputBox("Proc açıklaması", "Açıklama", _Aciklama);

            string conStr = txtConStr.Text.Trim();

            DataTable table = new DataTable();
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = new SqlCommand(secilenProcAdi, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            cmd.CommandType = CommandType.StoredProcedure;

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                if (item.Cells["veri"].Value == null)
                {
                    MessageBox.Show("Parametlere ait tüm veriler girilmelidir.");
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(item.Cells["veri"].Value.ToString()))
                    {
                        MessageBox.Show("Parametlere ait tüm veriler girilmelidir.");
                        return;
                    }
                }

                cmd.Parameters.Add(
                    parameterName: item.Cells["PARAMETER_NAME"].Value.ToString(),
                    sqlDbType: tipGetir(item),
                    size: Convert.ToInt32(item.Cells["CHARACTER_MAXIMUM_LENGTH"].Value)
                                    ).Value = item.Cells["veri"].Value;
            }

            da.Fill(table);

            ProcedureninNesneleriniOlustur(table);
            ProcedureninSelectMetodunuOlustur(table);
        }

        private readonly string[] SqlServerTypes = { "bigint", "binary", "bit", "char", "date", "datetime", "datetime2", "datetimeoffset", "decimal", "filestream", "float", "geography", "geometry", "hierarchyid", "image", "int", "money", "nchar", "ntext", "numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar", "xml" };
        private readonly string[] CSharpTypes = { "long", "byte[]", "bool", "char", "DateTime", "DateTime", "DateTime", "DateTimeOffset", "decimal", "byte[]", "double", "Microsoft.SqlServer.Types.SqlGeography", "Microsoft.SqlServer.Types.SqlGeometry", "Microsoft.SqlServer.Types.SqlHierarchyId", "byte[]", "int", "decimal", "string", "string", "decimal", "string", "Single", "byte[]", "DateTime", "short", "decimal", "object", "string", "TimeSpan", "byte[]", "byte", "Guid", "bite[]", "string", "string" };

        public string ConvertSqlServerFormatToCSharp(string typeName)
        {
            var index = Array.IndexOf(SqlServerTypes, typeName);

            return index > -1
                ? CSharpTypes[index]
                : "object";
        }

        public string ConvertCSharpFormatToSqlServer(string typeName)
        {
            var index = Array.IndexOf(CSharpTypes, typeName);

            return index > -1
                ? SqlServerTypes[index]
                : null;
        }

        private void ProcedureninSelectMetodunuOlustur(DataTable table)
        {
            char kacis = '"';

            StringBuilder sb = new StringBuilder();

            if (checkDefaultConString.Checked)
            {
                if (checkDefaultConString.CheckState != CheckState.Indeterminate)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("string conString = " + kacis + txtConStr.Text.Trim() + kacis + ";");
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            if (checkReadValue.Checked)
            {
                sb.Append(Environment.NewLine);
                sb.Append("private static object ReadValue(DataRow item, string name, string nullable)");
                sb.Append(Environment.NewLine);
                sb.Append("{");
                sb.Append(Environment.NewLine);
                sb.Append("    if (nullable == " + kacis + "YES" + kacis + ")");
                sb.Append(Environment.NewLine);
                sb.Append("        return ((item[name] == DBNull.Value) ? string.Empty : item[name]);");
                sb.Append(Environment.NewLine);
                sb.Append("    else");
                sb.Append(Environment.NewLine);
                sb.Append("        return item[name];");
                sb.Append(Environment.NewLine);
                sb.Append("}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            sb.Append("#region " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("private List<" + secilenProcAdi + "> _" + secilenProcAdi.ToLowerInvariant() + " = new List<" + secilenProcAdi + ">();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"///" + _Aciklama);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("public void " + secilenProcAdi + "(");
            sb.Append("string conStr ");

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                sb.Append(
                    ", "
                    + ConvertSqlServerFormatToCSharp(item.Cells["DATA_TYPE"].Value.ToString().Trim())
                    + " "
                    + item.Cells["PARAMETER_NAME"].Value
                    +
                    " = "
                    + kacis
                    + item.Cells["VERI"].Value
                    + kacis
                    ).Replace("@", "");
            }

            sb.Append(")");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using (DataTable dataTable = new DataTable())");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using(SqlConnection cn = new SqlConnection(conStr))");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("SqlCommand sqlCommand = new SqlCommand();");
            sb.Append(Environment.NewLine);
            sb.Append("SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.Connection = cn;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandType = CommandType.StoredProcedure;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandText = " + kacis + secilenProcAdi + kacis + "; ");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                sb.Append(
                    "sqlCommand.Parameters.AddWithValue("
                    + kacis
                    + item.Cells["PARAMETER_NAME"].Value.ToString()
                    + kacis
                    + ", "
                    + item.Cells["PARAMETER_NAME"].Value.ToString().Replace("@", "")
                    + ");"
                    );
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            sb.Append("cn.Open();");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.SelectCommand = sqlCommand;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.Fill(dataTable);");
            sb.Append(Environment.NewLine);
            sb.Append("cn.Close();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("_" + secilenProcAdi.ToLowerInvariant() + ".Clear();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("foreach (DataRow item in dataTable.Rows)");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("_" + secilenProcAdi.ToLowerInvariant() + ".Add(new " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataColumn item in table.Columns)
            {
                sb.Append(
                        item.ColumnName
                        + "=("
                        + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.DataType.ToString().ToLower().Replace("system.", ""))
                        + ")"
                        + "ReadValue(item,"
                        + kacis
                        + item.ColumnName
                        + kacis
                        + ", "
                        + kacis
                        + ((item.AllowDBNull) ? "YES" : "NO")
                        + kacis
                        + "),"
                        );

                sb.Append(Environment.NewLine);
            }

            string s = sb.ToString();
            sb.Clear();
            s = s.ToString().Substring(0, s.Length - 3);
            sb.Append(s);

            sb.Append(Environment.NewLine);
            sb.Append("});");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("#endregion");

            richProcunMetodu.Text = sb.ToString();
        }

        private void ProcedureninNesneleriniOlustur(DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"/// " + _Aciklama);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("public class " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataColumn item in table.Columns)
            {
                sb.Append(
                        "    public "
                        +
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.DataType.ToString().ToLower().Replace("system.", "")) + ((item.AllowDBNull) ? "?" : "")
                        + " "
                        + item.ColumnName
                        + "  { get; set; }");
                sb.Append(Environment.NewLine);
            }

            sb.Append("}");

            richProcunNesneleri.Text = sb.ToString();
        }

        private SqlDbType tipGetir(DataGridViewRow item)
        {
            SqlDbType d = SqlDbType.Binary;

            Array enumValueArray = Enum.GetValues(typeof(SqlDbType));

            foreach (int enumValue in enumValueArray)
            {
                if (Enum.GetName(typeof(SqlDbType), enumValue).ToLower() == item.Cells["DATA_TYPE"].Value.ToString().Trim().ToLower())
                {
                    int i = enumValue;

                    d = (SqlDbType)enumValue;
                }
            }

            return d;
        }

        private void btnSorgulaVeMetdolariOlustur_Click(object sender, EventArgs e)
        {
            string conStr = txtConStr.Text.Trim();

            DataTable table = new DataTable();
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = new SqlCommand(txtQuerry.Text.Trim(), con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            da.Fill(table);

            string tabloAdi = Microsoft.VisualBasic.Interaction.InputBox("Metod başlığında ve oluşturulacak list nesnesinde görünecek isim. (Boşluk bırakma yerine alt tire kullan, en az 3 karakter kullan!)", "İsim");

            if (string.IsNullOrEmpty(tabloAdi) && tabloAdi.Length > 3)
                return;

            string aciklama = Microsoft.VisualBasic.Interaction.InputBox("Querry açıklaması...", "Querry açıklaması");

            if (checkSorguyuTekClassYap.Checked)
            {
                StringBuilder stringAnaNesne = new StringBuilder();
                stringAnaNesne.Append(tabloAdi + " " + tabloAdi.Substring(0, 3).ToLower() + " = new " + tabloAdi + "();");
                stringAnaNesne.Append(Environment.NewLine);
                stringAnaNesne.Append("List <" + tabloAdi + "> " + tabloAdi.ToLower() + "_ = new List<" + tabloAdi + ">();");
                stringAnaNesne.Append(Environment.NewLine);

                txtQuerryTekClassAnaNesneler.Text = stringAnaNesne.ToString();

                txtQuerryTekClassAtama.Text = tabloAdi.ToLower() + "_ = " + tabloAdi.Substring(0, 3).ToLower() + "." + tabloAdi + "_SELECT();";

                QuerryninNesneleriniOlusturTekClass(table, aciklama, tabloAdi);
                QuerryninMetodunuOlusturTekClass(table, tabloAdi);
            }
            else
            {
                QuerryninNesneleriniOlustur(table, aciklama, tabloAdi);
                QuerryninMetodunuOlustur(table, tabloAdi);
            }
        }

        private void QuerryninMetodunuOlustur(DataTable table, string secilenProcAdi)
        {
            char kacis = '"';

            StringBuilder sb = new StringBuilder();

            if (checkDefaultConString.Checked)
            {
                if (checkDefaultConString.CheckState != CheckState.Indeterminate)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("string conString = " + kacis + txtConStr.Text.Trim() + kacis + ";");
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            if (checkReadValue.Checked)
            {
                sb.Append(Environment.NewLine);
                sb.Append("private static object ReadValue(DataRow item, string name, string nullable)");
                sb.Append(Environment.NewLine);
                sb.Append("{");
                sb.Append(Environment.NewLine);
                sb.Append("    if (nullable == " + kacis + "YES" + kacis + ")");
                sb.Append(Environment.NewLine);
                sb.Append("        return ((item[name] == DBNull.Value) ? string.Empty : item[name]);");
                sb.Append(Environment.NewLine);
                sb.Append("    else");
                sb.Append(Environment.NewLine);
                sb.Append("        return item[name];");
                sb.Append(Environment.NewLine);
                sb.Append("}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            sb.Append("#region " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("private List<" + secilenProcAdi + "> _" + secilenProcAdi.ToLowerInvariant() + " = new List<" + secilenProcAdi + ">();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"///" + _Aciklama);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("public void " + secilenProcAdi.ToUpper() + "_SELECT(");
            sb.Append("string conStr ");

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                sb.Append(
                    ", "
                    + ConvertSqlServerFormatToCSharp(item.Cells["DATA_TYPE"].Value.ToString().Trim())
                    + " "
                    + item.Cells["PARAMETER_NAME"].Value
                    +
                    " = "
                    + kacis
                    + item.Cells["VERI"].Value
                    + kacis
                    ).Replace("@", "");
            }

            sb.Append(")");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using (DataTable dataTable = new DataTable())");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using(SqlConnection cn = new SqlConnection(conStr))");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("SqlCommand sqlCommand = new SqlCommand();");
            sb.Append(Environment.NewLine);
            sb.Append("SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.Connection = cn;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandType = CommandType.Text;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandText = " + kacis + txtQuerry.Text.Trim() + kacis + "; ");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                sb.Append(
                    "sqlCommand.Parameters.AddWithValue("
                    + kacis
                    + item.Cells["PARAMETER_NAME"].Value.ToString()
                    + kacis
                    + ", "
                    + item.Cells["PARAMETER_NAME"].Value.ToString().Replace("@", "")
                    + ");"
                    );
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            sb.Append("cn.Open();");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.SelectCommand = sqlCommand;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.Fill(dataTable);");
            sb.Append(Environment.NewLine);
            sb.Append("cn.Close();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("_" + secilenProcAdi.ToLowerInvariant() + ".Clear();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("foreach (DataRow item in dataTable.Rows)");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("_" + secilenProcAdi.ToLowerInvariant() + ".Add(new " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataColumn item in table.Columns)
            {
                sb.Append(
                        item.ColumnName
                        + "=("
                        + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.DataType.ToString().ToLower().Replace("system.", "")).Replace("Datetime", "DateTime")
                        + ")"
                        + "ReadValue(item,"
                        + kacis
                        + item.ColumnName
                        + kacis
                        + ", "
                        + kacis
                        + ((item.AllowDBNull) ? "YES" : "NO")
                        + kacis
                        + "),"
                        );

                sb.Append(Environment.NewLine);
            }

            string s = sb.ToString();
            sb.Clear();
            s = s.ToString().Substring(0, s.Length - 3);
            sb.Append(s);

            sb.Append(Environment.NewLine);
            sb.Append("});");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("#endregion");

            txtQuerryMetod.Text += sb.ToString();
        }

        private void QuerryninNesneleriniOlustur(DataTable table, string aciklama, string className)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(aciklama))
            {
                sb.Append(@"///<summary>");
                sb.Append(Environment.NewLine);
                sb.Append(@"/// " + aciklama);
                sb.Append(Environment.NewLine);
                sb.Append(@"///</summary>");
                sb.Append(Environment.NewLine);
            }

            sb.Append("public class " + className);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataColumn item in table.Columns)
            {
                string s = (
                        "    public "
                        +
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.DataType.ToString().ToLower().Replace("system.", "")) + ((item.AllowDBNull) ? "?" : "")
                        + " "
                        + item.ColumnName
                        + "  { get; set; }");

                s = s
                    .Replace("String?", "String")
                    .Replace("Datetime?", "DateTime?");

                sb.Append(s);

                sb.Append(Environment.NewLine);
            }

            sb.Append("}");

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            txtQuerryMetod.Text = sb.ToString();
        }

        private void QuerryninNesneleriniOlusturTekClass(DataTable table, string aciklama, string className)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(aciklama))
            {
                sb.Append(@"///<summary>");
                sb.Append(Environment.NewLine);
                sb.Append(@"/// " + aciklama);
                sb.Append(Environment.NewLine);
                sb.Append(@"///</summary>");
                sb.Append(Environment.NewLine);
            }

            sb.Append("public class " + className);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataColumn item in table.Columns)
            {
                string s = (
                        "    public "
                        +
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.DataType.ToString().ToLower().Replace("system.", "")) + ((item.AllowDBNull) ? "?" : "")
                        + " "
                        + item.ColumnName
                        + "  { get; set; }");

                s = s
                    .Replace("String?", "String")
                    .Replace("Datetime?", "DateTime?");

                sb.Append(s);

                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            txtQuerryMetod.Text = sb.ToString();
        }

        private void QuerryninMetodunuOlusturTekClass(DataTable table, string secilenProcAdi)
        {
            char kacis = '"';

            StringBuilder sb = new StringBuilder();

            if (checkDefaultConString.Checked)
            {
                if (checkDefaultConString.CheckState != CheckState.Indeterminate)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("string conString = " + kacis + txtConStr.Text.Trim() + kacis + ";");
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                }
            }

            if (checkReadValue.Checked)
            {
                sb.Append(Environment.NewLine);
                sb.Append("private static object ReadValue(DataRow item, string name, string nullable)");
                sb.Append(Environment.NewLine);
                sb.Append("{");
                sb.Append(Environment.NewLine);
                sb.Append("    if (nullable == " + kacis + "YES" + kacis + ")");
                sb.Append(Environment.NewLine);
                sb.Append("        return ((item[name] == DBNull.Value) ? string.Empty : item[name]);");
                sb.Append(Environment.NewLine);
                sb.Append("    else");
                sb.Append(Environment.NewLine);
                sb.Append("        return item[name];");
                sb.Append(Environment.NewLine);
                sb.Append("}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            sb.Append("#region " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("private List<" + secilenProcAdi + "> _" + secilenProcAdi.ToLowerInvariant() + " = new List<" + secilenProcAdi + ">();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(@"///<summary>");
            sb.Append(Environment.NewLine);
            sb.Append(@"///" + _Aciklama);
            sb.Append(Environment.NewLine);
            sb.Append(@"///</summary>");
            sb.Append(Environment.NewLine);
            sb.Append("public List<" + secilenProcAdi + "> " + secilenProcAdi.ToUpper() + "_SELECT(");
            sb.Append("string sqlConString =  " + kacis + kacis);

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                sb.Append(
                    ", "
                    + ConvertSqlServerFormatToCSharp(item.Cells["DATA_TYPE"].Value.ToString().Trim())
                    + " "
                    + item.Cells["PARAMETER_NAME"].Value
                    +
                    " = "
                    + kacis
                    + item.Cells["VERI"].Value
                    + kacis
                    ).Replace("@", "");
            }

            sb.Append(")");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append(@"string s;");
            sb.Append(Environment.NewLine);
            sb.Append("List<" + secilenProcAdi + "> _" + secilenProcAdi.ToLowerInvariant() + " = new List<" + secilenProcAdi + ">();");
            sb.Append(Environment.NewLine);
            sb.Append(@"if (!string.IsNullOrEmpty(sqlConString))");
            sb.Append(Environment.NewLine);
            sb.Append(@"s = sqlConString;");
            sb.Append(Environment.NewLine);
            sb.Append(@"else");
            sb.Append(Environment.NewLine);
            sb.Append(@"s = conString;");
            sb.Append(Environment.NewLine);
            sb.Append("using (DataTable dataTable = new DataTable())");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("using(SqlConnection cn = new SqlConnection(s))");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("SqlCommand sqlCommand = new SqlCommand();");
            sb.Append(Environment.NewLine);
            sb.Append("SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.Connection = cn;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandType = CommandType.Text;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlCommand.CommandText = " + kacis + txtQuerry.Text.Trim() + kacis + "; ");
            sb.Append(Environment.NewLine);

            foreach (DataGridViewRow item in dgvProcedureParametreleri.Rows)
            {
                sb.Append(
                    "sqlCommand.Parameters.AddWithValue("
                    + kacis
                    + item.Cells["PARAMETER_NAME"].Value.ToString()
                    + kacis
                    + ", "
                    + item.Cells["PARAMETER_NAME"].Value.ToString().Replace("@", "")
                    + ");"
                    );
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            sb.Append("cn.Open();");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.SelectCommand = sqlCommand;");
            sb.Append(Environment.NewLine);
            sb.Append("sqlDataAdapter.Fill(dataTable);");
            sb.Append(Environment.NewLine);
            sb.Append("cn.Close();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("_" + secilenProcAdi.ToLowerInvariant() + ".Clear();");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("foreach (DataRow item in dataTable.Rows)");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("_" + secilenProcAdi.ToLowerInvariant() + ".Add(new " + secilenProcAdi);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (DataColumn item in table.Columns)
            {
                sb.Append(
                        item.ColumnName
                        + "=("
                        + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.DataType.ToString().ToLower().Replace("system.", "")).Replace("Datetime", "DateTime")
                        + ")"
                        + "ReadValue(item,"
                        + kacis
                        + item.ColumnName
                        + kacis
                        + ", "
                        + kacis
                        + ((item.AllowDBNull) ? "YES" : "NO")
                        + kacis
                        + "),"
                        );

                sb.Append(Environment.NewLine);
            }

            string s = sb.ToString();
            sb.Clear();
            s = s.ToString().Substring(0, s.Length - 3);
            sb.Append(s);

            sb.Append(Environment.NewLine);
            sb.Append("});");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append(@"return _" + secilenProcAdi.ToLowerInvariant() + ";");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            sb.Append(Environment.NewLine);

            sb.Append("#endregion");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            txtQuerryMetod.Text += sb.ToString();
        }

        private void kopyalaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtQuerryMetod.Text);
        }

        private void txtQuerryTekClassAnaNesneler_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(((RichTextBox)sender).Text);
        }

        private void txtQuerryTekClassAtama_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(((TextBox)sender).Text);
        }

        private void txtQuerryMetod_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(((RichTextBox)sender).Text);
        }
    }
}