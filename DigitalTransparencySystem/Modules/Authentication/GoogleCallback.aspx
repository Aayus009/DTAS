<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GoogleCallback.aspx.cs" Inherits="DigitalTransparencySystem.Modules.Authentication.GoogleCallback" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <title>Signing in | DTAS</title>
    <style>
        body { font-family: "Hanken Grotesk", sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; background: #f4f7fb; color: #0b1f3a; }
        p { font-weight: 700; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <p>Finishing Google sign-in...</p>
        <asp:Label ID="lblError" runat="server" ForeColor="#9b1c1c"></asp:Label>
    </form>
</body>
</html>
