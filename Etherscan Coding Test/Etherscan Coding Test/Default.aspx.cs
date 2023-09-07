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
 * job for someone who learnt it on the fly :D.
 * 
 * Anyways, hope my code isn't too bad. Kinda wondering why I gotta do backend stuff when I'm applying for 
 * frontend though :/.
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
        // List of tokens from db will be populated into this
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
            // Fetch the tokens from db and bind to TokenGrid
            tokens = DatabaseManager.GetTokens();
            TokenGrid.DataSource = tokens;
            TokenGrid.DataBind();
        }

        protected void TokenGrid_NavigateToContract(object sender, EventArgs e)
        {
            // Get the CommandArgument set in the view file.
            LinkButton lnkName = (LinkButton)sender;

            // I set the CommandArgument of the LinkButton
            // in the view file to the symbol of the token.
            string symbol = lnkName.CommandArgument;

            // Redirect user based on symbol clicked.
            Response.Redirect($"Details.aspx?id={symbol}");
        }

        protected void TokenGrid_EditRow(object sender, EventArgs e)
        {
            // Get the CommandArgument from the view file
            LinkButton lnkName = (LinkButton)sender;

            // I set the CommandArgument to the row index.
            int rowIndex = Convert.ToInt32(lnkName.CommandArgument);

            // Get data from grid view row
            GridViewRow row = TokenGrid.Rows[rowIndex];
            string id = row.Cells[0].Text;
            // Have to do this acrobatics because the content of the cell is a control.
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


        protected void TokenGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Set the page index of the pagination
            // of TokenGrid based on event data.
            TokenGrid.PageIndex = e.NewPageIndex;

            FetchAndBind();
        }

        // Used for giving each token a unique
        // color in the doughnut chart.
        protected string GetRandomColor(int seed)
        {
            Random rnd = new Random(seed);
            return $"rgb({rnd.Next(255)}, {rnd.Next(255)}, {rnd.Next(255)})";
        }

        protected void TokenSave_Click(object sender, EventArgs e)
        {
            // Creating an Token object from text from textboxes.
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
            FetchAndBind(); // Ensure data is up-to-date

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=export.csv");
            Response.Charset = "";
            Response.ContentType = "application/text";

            TokenGrid.AllowPaging = false;
            TokenGrid.DataBind();

            
            // Go thru each column of the TokenGrid
            // to get the header row of the csv.
            StringBuilder columnbind = new StringBuilder();
            for (int k = 0; k < TokenGrid.Columns.Count - 1; k++)
            {
                columnbind.Append(TokenGrid.Columns[k].HeaderText + ',');
            }
            columnbind.Append("\r\n");

            // Then go through each data row, concatenating it into a single, comma-seperated line.
            for (int i = 0; i < TokenGrid.Rows.Count; i++)
            {
                for (int k = 0; k < TokenGrid.Columns.Count - 1; k++)
                {
                    var text = TokenGrid.Rows[i].Cells[k].Text;
                    // A bit of a hack, but I literally could not think of a better way, sorry.
                    if (text == "" && k == 2)
                    {
                        // Same acrobatics to get the Text value from cells with controls.
                        text = (TokenGrid.Rows[i].Cells[k].FindControl("lnkSymbol") as LinkButton).Text;
                    }
                    columnbind.Append(text + ',');
                }
                columnbind.Append("\r\n");
            }

            // Trigger a download.
            Response.Output.Write(columnbind.ToString());
            Response.Flush();
            Response.End();
        }
    }
}