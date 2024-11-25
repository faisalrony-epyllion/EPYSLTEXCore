<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportView.aspx.cs" Inherits="EPYSLTEX.Web.Reports.ReportView" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="../Content/css/report-viewer.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div class="Reportpage">
            <rsweb:ReportViewer ID="rpViewer" runat="server" Width="100%" Height="100%" AsyncRendering="False" SizeToReportContent="True"></rsweb:ReportViewer>
        </div>

        <script type="text/javascript">
            (function () {
                var isWebKit = !!window.webkitURL;
                var element = document.getElementById("rpViewer_ctl09");
                if (isWebKit) {
                    if (element) {
                        element.style.overflow = "visible";
                    }
                }
            })();
        </script>
    </form>
</body>
</html>
