<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Etherscan_Coding_Test._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <%-- 
        The default GridView pager looks terrible, but the formatting is not the same as Bootstrap's.
        Used some guy's styling online and it works great.
    --%>
    <style>
        .pagination-ys {
            /*display: inline-block;*/
            padding-left: 0;
            margin: 20px 0;
            border-radius: 4px;
        }

            .pagination-ys table > tbody > tr > td {
                display: inline;
            }

                .pagination-ys table > tbody > tr > td > a,
                .pagination-ys table > tbody > tr > td > span {
                    position: relative;
                    float: left;
                    padding: 8px 12px;
                    line-height: 1.42857143;
                    text-decoration: none;
                    color: #0d6efd;
                    background-color: #ffffff;
                    border: 1px solid #dddddd;
                    margin-left: -1px;
                }

                .pagination-ys table > tbody > tr > td > span {
                    position: relative;
                    float: left;
                    padding: 8px 12px;
                    line-height: 1.42857143;
                    text-decoration: none;
                    margin-left: -1px;
                    z-index: 2;
                    color: #aea79f;
                    background-color: #f5f5f5;
                    border-color: #dddddd;
                    cursor: default;
                }

                .pagination-ys table > tbody > tr > td:first-child > a,
                .pagination-ys table > tbody > tr > td:first-child > span {
                    margin-left: 0;
                    border-bottom-left-radius: 4px;
                    border-top-left-radius: 4px;
                }

                .pagination-ys table > tbody > tr > td:last-child > a,
                .pagination-ys table > tbody > tr > td:last-child > span {
                    border-bottom-right-radius: 4px;
                    border-top-right-radius: 4px;
                }

                .pagination-ys table > tbody > tr > td > a:hover,
                .pagination-ys table > tbody > tr > td > span:hover,
                .pagination-ys table > tbody > tr > td > a:focus,
                .pagination-ys table > tbody > tr > td > span:focus {
                    color: #0d6efd;
                    background-color: #eeeeee;
                    border-color: #dddddd;
                }
    </style>

    <main class="border rounded-1 p-4">
        <section class="row" aria-labelledby="tokenInfo">
            <h1 id="tokenInfo">Save / Update Token</h1>

            <div class="col-md-4">
                <asp:TextBox runat="server" type="hidden" Placeholder="Id" class="form-control mw-100" ID="txtId" />

                <div class="mb-3">
                    <label for="txtName" class="form-label">Name</label>
                    <asp:TextBox runat="server" ClientIDMode="Static" type="text" Placeholder="Name" class="form-control mw-100" ID="txtName" />
                </div>

                <div class="mb-3">
                    <label for="txtSymbol" class="form-label">Symbol</label>
                    <asp:TextBox runat="server" ClientIDMode="Static" type="text" placeholder="Symbol" class="form-control mw-100" ID="txtSymbol" />
                </div>

                <div class="mb-3">
                    <label for="txtContractAddress" class="form-label">Contract Address</label>
                    <asp:TextBox runat="server" ClientIDMode="Static" type="text" placeholder="Contract Address" class="form-control mw-100" ID="txtContractAddress" />
                </div>

                <div class="mb-3">
                    <label for="txtTotalSupply" class="form-label">Total Supply</label>
                    <asp:TextBox runat="server" ClientIDMode="Static" type="number" placeholder="Total Supply" min="0" class="form-control mw-100" ID="txtTotalSupply" />
                </div>

                <div class="mb-3">
                    <label for="txtTotalHolders" class="form-label">Total Holders</label>
                    <asp:TextBox runat="server" ClientIDMode="Static" type="number" placeholder="Total Holders" min="0" class="form-control mw-100" ID="txtTotalHolders" />
                </div>

                <asp:Button runat="server" ID="btnAdd" Text="Add" OnClick="TokenSave_Click" class="btn btn-primary" />
                <asp:Button runat="server" ID="btnReset" Text="Reset" OnClick="TokenReset_Click" class="btn btn-secondary" />
            </div>
            <div class="col-md-8">
                <canvas id="tokenChart" class="mh-100"></canvas>
            </div>
        </section>

        <hr class="my-4">


        <section aria-labelledby="tokenTable">
            <div class="table-responsive">
                <asp:GridView
                    AllowPaging="true"
                    AllowSorting="true"
                    PageSize="10"
                    CssClass="table align-middle"
                    ID="GridView1"
                    runat="server"
                    AutoGenerateColumns="false"
                    OnPageIndexChanging="GridView1_PageIndexChanging" PagerStyle-CssClass="pagination-ys">
                    <Columns>
                        <asp:BoundField DataField="Id" HeaderText="Id" HeaderStyle-CssClass="collapse" ItemStyle-CssClass="collapse" />
                        <asp:BoundField DataField="Rank" HeaderText="Rank" />
                        <asp:TemplateField HeaderText="Symbol">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkSymbol" CssClass="btn btn-link" runat="server" Text='<%# Eval("Symbol") %>' OnClick="GridView1_NavigateToContract" CommandArgument='<%# Eval("Symbol") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="ContractAddress" HeaderText="Contract Address" />
                        <asp:BoundField DataField="TotalHolders" HeaderText="Total Holders" />
                        <asp:BoundField DataField="TotalSupply" HeaderText="Total Supply" />
                        <asp:BoundField DataField="TotalSupplyPercentage" HeaderText="Total Supply %" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton CssClass="btn btn-link" runat="server" Text="Edit" OnClick="GridView1_EditRow" CommandArgument='<%# Container.DataItemIndex %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:Button runat="server" ID="btnExport" Text="Export" OnClick="btnExport_Click" CssClass="btn btn-primary"/>
            </div>
        </section>

    </main>
    <script>
        const ctx = document.getElementById('tokenChart');

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: [<%= string.Join(", ", tokens.Select(t => $"'{t.Name}'")) %>],
                datasets: [{
                    label: 'Total Supply',
                    data: [<%= string.Join(", ", tokens.Select(t => t.TotalSupply)) %>],
                    backgroundColor: [
                        <%= string.Join(", ", tokens.Select(t => $"'{GetRandomColor(t.Rank)}'")) %>
                    ],
                    hoverOffset: 4
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    </script>

</asp:Content>
