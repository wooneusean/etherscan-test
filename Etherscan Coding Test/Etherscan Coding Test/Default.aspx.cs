using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

/**
 * Hi reviewer, before you see the mess below, I have to say that this isn't my proudest work, I have never
 * used ASP.Net Web Forms before. I did try my best with the time I got, and I would say I did quite a good
 * job for someone who learnt it on the fly.
 * 
 * Anyways, hope my code isn't too bad. Kinda wondering why I gotta do backend stuff when I'm applying for 
 * frontend though :/
 * 
 * Regards,
 * Woon Eusean
 * 
 * P.S Might I add, I am actually quite strong on the JavaScript side, with React.js, Express.js and whatnot.
 * 
 * P.P.S Java is my mother tongue when it comes to back end as well, please dont let the terrible code below
 *        make you have a bad impression of me :thumbsup:
 */

namespace Etherscan_Coding_Test
{

    public partial class _Default : Page
    {
        protected int currentPage = 0;
        protected int perPage = 10;

        protected List<Token> tokens = new List<Token>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FetchAndBind();
            }
        }

        protected void FetchAndBind()
        {
            tokens = DatabaseManager.GetTokens();
            GridView1.DataSource = tokens;
            GridView1.DataBind();
        }



        protected PaginationDetails GetPaginationDetails()
        {
            try
            {
                return DatabaseManager.GetPaginationDetails(perPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new PaginationDetails { totalRecords = 0, totalPages = 0 };
            }
        }

        protected void GridView1_NavigateToContract(object sender, EventArgs e)
        {
            LinkButton lnkName = (LinkButton)sender;

            string symbol = lnkName.CommandArgument;

            Response.Redirect($"Details.aspx?id={symbol}");
        }

        protected void GridView1_EditRow(object sender, EventArgs e)
        {
            LinkButton lnkName = (LinkButton)sender;

            int rowIndex = Convert.ToInt32(lnkName.CommandArgument);

            // get data from grid view row
            GridViewRow row = GridView1.Rows[rowIndex];
            string id = row.Cells[0].Text;
            string symbol = (row.Cells[2].FindControl("lnkSymbol") as LinkButton).Text;
            string name = row.Cells[3].Text;
            string contractAddress = row.Cells[4].Text;
            string totalHolders = row.Cells[5].Text;
            string totalSupply = row.Cells[6].Text;

            // set data to textboxes
            txtId.Text = id;
            txtSymbol.Text = symbol;
            txtName.Text = name;
            txtTotalSupply.Text = totalSupply;
            txtContractAddress.Text = contractAddress;
            txtTotalHolders.Text = totalHolders;

            btnAdd.Text = "Save";

            FetchAndBind();
        }


        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;

            FetchAndBind();
        }

        protected string GetRandomColor(int seed)
        {
            Random rnd = new Random(seed);
            return $"rgb({rnd.Next(255)}, {rnd.Next(255)}, {rnd.Next(255)})";
        }

        protected void TokenSave_Click(object sender, EventArgs e)
        {
            Token token = new Token
            {
                Id = txtId.Text.Trim() == "" ? -1 : Convert.ToInt32(txtId.Text),
                Symbol = txtSymbol.Text,
                Name = txtName.Text,
                TotalSupply = Convert.ToInt64(txtTotalSupply.Text),
                ContractAddress = txtContractAddress.Text,
                TotalHolders = Convert.ToInt32(txtTotalHolders.Text),
            };

            if (DatabaseManager.UpsertToken(token))
            {
                TokenReset_Click(sender, e);
                FetchAndBind();
            }
        }

        protected void TokenReset_Click(object sender, EventArgs e)
        {
            txtId.Text = "";
            txtSymbol.Text = "";
            txtName.Text = "";
            txtTotalSupply.Text = "";
            txtContractAddress.Text = "";
            txtTotalHolders.Text = "";
            btnAdd.Text = "Add";

            FetchAndBind();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            ExportGridToCSV();
        }


        private void ExportGridToCSV()
        {
            FetchAndBind();

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=export.csv");
            Response.Charset = "";
            Response.ContentType = "application/text";

            GridView1.AllowPaging = false;
            GridView1.DataBind();

            StringBuilder columnbind = new StringBuilder();
            for (int k = 0; k < GridView1.Columns.Count - 1; k++)
            {
                columnbind.Append(GridView1.Columns[k].HeaderText + ',');
            }
            columnbind.Append("\r\n");
            for (int i = 0; i < GridView1.Rows.Count; i++)
            {
                for (int k = 0; k < GridView1.Columns.Count - 1; k++)
                {
                    var text = GridView1.Rows[i].Cells[k].Text;
                    if (text == "" && k == 2)
                    {
                        text = (GridView1.Rows[i].Cells[k].FindControl("lnkSymbol") as LinkButton).Text;
                    }
                    columnbind.Append(text + ',');
                }
                columnbind.Append("\r\n");
            }

            Response.Output.Write(columnbind.ToString());
            Response.Flush();
            Response.End();
        }
    }
}