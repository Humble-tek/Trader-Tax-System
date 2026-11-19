Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmTrader
    Public Sub New()
        InitializeComponent()
        ' Show a preview of the screen in the Visual Studio Design view only
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            BuildScreen()
        End If
    End Sub

    Private pnlContent As Panel
    Private navButtons As New List(Of Button)

    ' ================= STARTUP =================
    Private Sub frmTrader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Tax Collection System - Trader"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(900, 560)
        Me.Size = New Size(1100, 700)
        Me.BackColor = Color.White
        BuildScreen()
        ShowHome(Nothing, EventArgs.Empty)
    End Sub

    Private Sub frmTrader_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' Forget the trader when the dashboard closes
        CurrentTraderID = 0
        CurrentFullName = ""
        CurrentBusinessType = ""
        CurrentPhone = ""
        CurrentAddress = ""
        CurrentGender = ""
        CurrentUsername = ""
    End Sub

    ' ================= SCREEN LAYOUT =================
    Private Sub BuildScreen()
        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.ColumnCount = 2
        root.RowCount = 2
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 70))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Me.Controls.Add(root)

        ' ---------- Header ----------
        Dim header As New Panel()
        header.Dock = DockStyle.Fill
        header.BackColor = PrimaryColor

        Dim lblTitle As New Label()
        lblTitle.Text = "Tax Collection System"
        lblTitle.ForeColor = Color.White
        lblTitle.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblTitle.AutoSize = True

        Dim flag As Panel = NewGhanaFlag(54)
        flag.Location = New Point(20, 17)
        header.Controls.Add(flag)

        lblTitle.Location = New Point(88, 18)
        header.Controls.Add(lblTitle)

        Dim lblUser As New Label()
        lblUser.Text = "Welcome, " & CurrentFullName
        lblUser.ForeColor = Color.White
        lblUser.Font = New Font("Segoe UI", 12)
        lblUser.AutoSize = True
        lblUser.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblUser.Location = New Point(header.Width - 320, 24)
        header.Controls.Add(lblUser)

        root.Controls.Add(header, 0, 0)
        root.SetColumnSpan(header, 2)

        ' ---------- Left menu ----------
        Dim menu As New FlowLayoutPanel()
        menu.Dock = DockStyle.Fill
        menu.FlowDirection = FlowDirection.TopDown
        menu.WrapContents = False
        menu.BackColor = Color.FromArgb(30, 40, 55)
        menu.Padding = New Padding(0, 10, 0, 0)

        menu.Controls.Add(NavButton("Home", AddressOf ShowHome))
        menu.Controls.Add(NavButton("Pay Tax", AddressOf ShowPayTax))
        menu.Controls.Add(NavButton("Tax History", AddressOf ShowHistory))
        menu.Controls.Add(NavButton("Complaints", AddressOf ShowComplaints))
        menu.Controls.Add(NavButton("Profile", AddressOf ShowProfile))

        Dim btnLogout As Button = NavButton("Logout", AddressOf Logout_Click)
        btnLogout.BackColor = DangerColor
        btnLogout.Margin = New Padding(0, 30, 0, 0)
        navButtons.Remove(btnLogout)
        menu.Controls.Add(btnLogout)

        root.Controls.Add(menu, 0, 1)

        ' ---------- Content area ----------
        pnlContent = New Panel()
        pnlContent.Dock = DockStyle.Fill
        pnlContent.BackColor = Color.FromArgb(244, 246, 250)
        pnlContent.Padding = New Padding(30)
        pnlContent.AutoScroll = True
        root.Controls.Add(pnlContent, 1, 1)
    End Sub

    Private Function NavButton(text As String, handler As EventHandler) As Button
        Dim b As New Button()
        b.Text = "   " & text
        b.TextAlign = ContentAlignment.MiddleLeft
        b.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        b.Width = 220
        b.Height = 50
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.BackColor = Color.FromArgb(30, 40, 55)
        b.ForeColor = Color.White
        b.Margin = New Padding(0)
        b.Cursor = Cursors.Hand
        AddHandler b.Click, handler
        navButtons.Add(b)
        Return b
    End Function

    Private Sub Highlight(active As Button)
        For Each b As Button In navButtons
            b.BackColor = Color.FromArgb(30, 40, 55)
        Next
        If active IsNot Nothing AndAlso navButtons.Contains(active) Then
            active.BackColor = PrimaryColor
        End If
    End Sub

    Private Sub ClearContent()
        pnlContent.Controls.Clear()
    End Sub

    ' Puts a finished page into the content area
    Private Sub LoadView(view As Control)
        pnlContent.Controls.Clear()
        view.Dock = DockStyle.Fill
        pnlContent.Controls.Add(view)
    End Sub

    ' ================= HOME =================
    Private Sub ShowHome(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        ClearContent()

        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.Controls.Add(NewLabel("Welcome, " & CurrentFullName, 22, True))
        card.Controls.Add(NewLabel("Business: " & CurrentBusinessType, 12))
        card.Controls.Add(NewLabel("Payment phone number: " & CurrentPhone, 12))
        card.Controls.Add(NewLabel("Use the menu on the left to pay tax, view your tax history," & vbCrLf &
                                   "lodge a complaint, or see your profile.", 11))
        pnlContent.Controls.Add(card)
    End Sub

    ' ================= PROFILE =================
    Private Sub ShowProfile(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        ClearContent()

        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.Controls.Add(NewLabel("My Profile", 22, True))
        card.Controls.Add(NewLabel("Your registration details", 11))

        Dim grid As New TableLayoutPanel()
        grid.ColumnCount = 2
        grid.AutoSize = True
        grid.Margin = New Padding(3, 20, 3, 3)
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))

        AddProfileRow(grid, "Full Name:", CurrentFullName)
        AddProfileRow(grid, "Business Type:", CurrentBusinessType)
        AddProfileRow(grid, "Phone Number:", CurrentPhone)
        AddProfileRow(grid, "House Address:", CurrentAddress)
        AddProfileRow(grid, "Gender:", CurrentGender)
        AddProfileRow(grid, "Username:", CurrentUsername)

        card.Controls.Add(grid)

        Dim btnChange As Button = NewButton("Change Password", Color.FromArgb(30, 70, 140))
        btnChange.Margin = New Padding(3, 20, 3, 3)
        AddHandler btnChange.Click, Sub(s, args)
                                        Using f As New frmChangePassword(False)
                                            f.ShowDialog(Me)
                                        End Using
                                    End Sub
        card.Controls.Add(btnChange)
        pnlContent.Controls.Add(card)
    End Sub

    Private Sub AddProfileRow(grid As TableLayoutPanel, caption As String, value As String)
        Dim r As Integer = grid.RowCount
        grid.RowCount = r + 1
        grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim lblCaption As Label = NewLabel(caption, 12, True)
        lblCaption.Margin = New Padding(3, 8, 40, 8)
        grid.Controls.Add(lblCaption, 0, r)

        Dim lblValue As Label = NewLabel(value, 12)
        lblValue.Margin = New Padding(3, 8, 3, 8)
        grid.Controls.Add(lblValue, 1, r)
    End Sub

    ' ================= PAGES BUILT IN LATER STAGES =================
    Private Sub ShowPayTax(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucPayTax())
    End Sub

    Private Sub ShowHistory(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucTaxHistory())
    End Sub

    Private Sub ShowComplaints(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucComplaints())
    End Sub

    Private Sub ShowPlaceholder(title As String, stage As String)
        ClearContent()
        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.Controls.Add(NewLabel(title, 22, True))
        card.Controls.Add(NewLabel("This page is built in " & stage & ".", 12))
        pnlContent.Controls.Add(card)
    End Sub

    ' ================= LOGOUT =================
    Private Sub Logout_Click(sender As Object, e As EventArgs)
        If MessageBox.Show("Do you want to log out?", "Logout",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Me.Close()
        End If
    End Sub

End Class