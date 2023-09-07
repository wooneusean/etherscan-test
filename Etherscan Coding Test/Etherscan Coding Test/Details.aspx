<%@ Page Title="Details Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="Etherscan_Coding_Test.Details" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="border rounded-1 p-4">
        <h3><%= currentToken.ContractAddress %></h3>
        <table class="table">
            <tbody>
                <tr>
                    <th scope="row">Price</th>
                    <td>$ <%= string.Format("{0:0.00###########}", currentToken.Price) %></td>
                </tr>
                <tr>
                    <th scope="row">Total Supply</th>
                    <td><%= $"{currentToken.TotalSupply} {currentToken.Symbol}" %></td>
                </tr>
                <tr>
                    <th scope="row">Total Holders</th>
                    <td colspan="2"><%= currentToken.TotalHolders %></td>
                </tr>
                <tr>
                    <th scope="row">Name</th>
                    <td colspan="2"><%= currentToken.Name %></td>
                </tr>
            </tbody>
        </table>
    </main>
</asp:Content>
