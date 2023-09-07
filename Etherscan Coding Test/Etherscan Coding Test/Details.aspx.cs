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
            string symbol = Request.QueryString["id"];

            currentToken = GetTokenBySymbol(symbol);
        }

        protected Token GetTokenBySymbol(string symbol)
        {
            return DatabaseManager.GetTokenBySymbol(symbol);
        }
    }
}