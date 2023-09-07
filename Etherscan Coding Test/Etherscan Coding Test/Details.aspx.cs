using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Etherscan_Coding_Test
{
    public partial class Details : System.Web.UI.Page
    {
        protected Token currentToken = new Token();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the `id` query string from the page URL.
            string symbol = Request.QueryString["id"];

            // Try to find the token in the db.
            currentToken = DatabaseManager.GetTokenBySymbol(symbol);
        }
    }
}