<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<form id="form1" runat="server">
    <div>
        <asp:Label ID="MyLabel" runat="server" />
    </div>
    <asp:FileUpload ID="MyFileUpload" runat="server" />
    <asp:Button ID="GoButton" Text="GO!" OnClick="GoButton_Click" runat="server" />
</form>
</body>
</html>
