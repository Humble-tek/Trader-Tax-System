Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms

Public Class frmAdmin
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
    Private Sub frmAdmin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Tax Collection System - Admin"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(1000, 600)
        Me.Size = New Size(1250, 740)
        Me.BackColor = Color.White
        BuildScreen()
        ShowTraders(navButtons(0), EventArgs.Empty)
    End Sub

    ' ================= SCREEN LAYOUT =================
    Private Sub BuildScreen()
        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.ColumnCount = 2
        root.RowCount = 2
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 70))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Me.Controls.Add(root)

        ' ---------- Header ----------
        Dim header As New Panel()
        header.Dock = DockStyle.Fill
        header.BackColor = Color.FromArgb(30, 70, 140)

        Dim lblUser As New Label()
        lblUser.Text = "Logged in as Administrator"
        lblUser.ForeColor = Color.White
        lblUser.Font = New Font("Segoe UI", 12)
        lblUser.AutoSize = False
        lblUser.Width = 320
        lblUser.TextAlign = ContentAlignment.MiddleRight
        lblUser.Padding = New Padding(0, 0, 20, 0)
        lblUser.Dock = DockStyle.Right
        header.Controls.Add(lblUser)

        Dim lblTitle As New Label()
        lblTitle.Text = "Tax Collection System - Admin Dashboard"
        lblTitle.ForeColor = Color.White
        lblTitle.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblTitle.AutoSize = False
        lblTitle.Width = 640
        lblTitle.TextAlign = ContentAlignment.MiddleLeft
        lblTitle.Padding = New Padding(90, 0, 0, 0)
        lblTitle.Dock = DockStyle.Left
        header.Controls.Add(lblTitle)
        Dim flag As Panel = NewGhanaFlag(54)
        flag.Location = New Point(20, 17)
        header.Controls.Add(flag)
        flag.BringToFront()

        root.Controls.Add(header, 0, 0)
        root.SetColumnSpan(header, 2)

        ' ---------- Left menu ----------
        Dim menu As New FlowLayoutPanel()
        menu.Dock = DockStyle.Fill
        menu.FlowDirection = FlowDirection.TopDown
        menu.WrapContents = False
        menu.BackColor = Color.FromArgb(30, 40, 55)
        menu.Padding = New Padding(0, 10, 0, 0)

        menu.Controls.Add(NavButton("Traders", AddressOf ShowTraders))
        menu.Controls.Add(NavButton("Monthly Payments", AddressOf ShowMonthly))
        menu.Controls.Add(NavButton("Complaints", AddressOf ShowComplaints))
        menu.Controls.Add(NavButton("Report", AddressOf ShowReport))
        menu.Controls.Add(NavButton("Change Password", AddressOf ShowChangePassword))

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
        pnlContent.Padding = New Padding(25)
        root.Controls.Add(pnlContent, 1, 1)
    End Sub

    Private Function NavButton(text As String, handler As EventHandler) As Button
        Dim b As New Button()
        b.Text = "   " & text
        b.TextAlign = ContentAlignment.MiddleLeft
        b.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        b.Width = 230
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
            active.BackColor = Color.FromArgb(30, 70, 140)
        End If
    End Sub

    Private Sub LoadView(view As Control)
        pnlContent.Controls.Clear()
        view.Dock = DockStyle.Fill
        pnlContent.Controls.Add(view)
    End Sub

    ' ================= PAGES =================
    Private Sub ShowTraders(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucAdminTraders())
    End Sub

    Private Sub ShowMonthly(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucAdminMonthly())
    End Sub

    Private Sub ShowComplaints(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucAdminComplaints())
    End Sub

    Private Sub ShowReport(sender As Object, e As EventArgs)
        Highlight(TryCast(sender, Button))
        LoadView(New ucAdminReport())
    End Sub

    Private Sub ShowPlaceholder(title As String, stage As String)
        pnlContent.Controls.Clear()
        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.Controls.Add(NewLabel(title, 22, True))
        card.Controls.Add(NewLabel("This page is built in " & stage & ".", 12))
        pnlContent.Controls.Add(card)
    End Sub

    Private Sub ShowChangePassword(sender As Object, e As EventArgs)
        Using f As New frmChangePassword(True)
            f.ShowDialog(Me)
        End Using
    End Sub


    ' ================= LOGOUT =================
    Private Sub Logout_Click(sender As Object, e As EventArgs)
        If MessageBox.Show("Do you want to log out?", "Logout",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Me.Close()
        End If
    End Sub

End Class