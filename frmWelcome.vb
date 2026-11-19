Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class frmWelcome
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        ' Preview in the Visual Studio Design view only
        If Me.DesignMode AndAlso Me.Controls.Count = 0 Then
            Try
                BuildScreen()
            Catch
            End Try
        End If
    End Sub
    Public Sub New()
        InitializeComponent()
        ' Show a preview of the screen in the Visual Studio Design view only
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            BuildScreen()
        End If
    End Sub

    Private txtAdminUser As TextBox
    Private txtAdminPass As TextBox
    Private txtTraderUser As TextBox
    Private txtTraderPass As TextBox
    Private pnlChoice As FlowLayoutPanel
    Private pnlLogin As FlowLayoutPanel

    ' ================= STARTUP =================
    Private Sub frmWelcome_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Tax Collection System"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(900, 560)
        Me.Size = New Size(1000, 620)
        Me.BackColor = Color.White

        BuildScreen()

        Try
            EnsureDefaultAdmin()
        Catch ex As Exception
            MessageBox.Show("Could not connect to the database: " & ex.Message,
                            "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ================= SCREEN LAYOUT =================
    Private Sub BuildScreen()
        Dim root As New TableLayoutPanel()
        root.Dock = DockStyle.Fill
        root.ColumnCount = 2
        root.RowCount = 1
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Me.Controls.Add(root)

        root.Controls.Add(BuildAdminSide(), 0, 0)
        root.Controls.Add(BuildTraderSide(), 1, 0)
    End Sub

    ' Places a card in the exact centre of a coloured area
    Private Function CenterCard(card As Control, background As Color) As Panel
        Dim host As New Panel()
        host.Dock = DockStyle.Fill
        host.BackColor = background

        Dim grid As New TableLayoutPanel()
        grid.Dock = DockStyle.Fill
        grid.ColumnCount = 1
        grid.RowCount = 3
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        grid.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        grid.RowStyles.Add(New RowStyle(SizeType.Percent, 50))

        card.Anchor = AnchorStyles.None
        grid.Controls.Add(card, 0, 1)
        host.Controls.Add(grid)
        Return host
    End Function

    Private Function NewCard() As FlowLayoutPanel
        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.AutoSizeMode = AutoSizeMode.GrowAndShrink
        card.Padding = New Padding(20)
        Return card
    End Function

    ' ---------- Left side: admin ----------
    Private Function BuildAdminSide() As Control
        Dim card As FlowLayoutPanel = NewCard()
        card.Controls.Add(NewLabel("Admin Login", 20, True))

        card.Controls.Add(NewLabel("Username:"))
        txtAdminUser = NewTextBox()
        card.Controls.Add(txtAdminUser)

        card.Controls.Add(NewLabel("Password:"))
        txtAdminPass = NewTextBox(True)
        card.Controls.Add(txtAdminPass)

        Dim btnLogin As Button = NewButton("Login")
        btnLogin.Margin = New Padding(3, 20, 3, 3)
        AddHandler btnLogin.Click, AddressOf AdminLogin_Click
        card.Controls.Add(btnLogin)

        Return CenterCard(card, Color.FromArgb(235, 245, 238))
    End Function

    ' ---------- Right side: trader ----------
    Private Function BuildTraderSide() As Control
        Dim blue As Color = Color.FromArgb(30, 70, 140)
        Dim grey As Color = Color.FromArgb(110, 110, 110)

        ' View 1: only Login and Register buttons
        pnlChoice = NewCard()
        pnlChoice.Controls.Add(NewLabel("Trader", 20, True))
        pnlChoice.Controls.Add(NewLabel("Please choose an option."))

        Dim btnShowLogin As Button = NewButton("Login", blue)
        btnShowLogin.Margin = New Padding(3, 20, 3, 3)
        AddHandler btnShowLogin.Click, AddressOf ShowLoginView
        pnlChoice.Controls.Add(btnShowLogin)

        Dim btnShowRegister As Button = NewButton("Register", PrimaryColor)
        btnShowRegister.Margin = New Padding(3, 10, 3, 3)
        AddHandler btnShowRegister.Click, AddressOf RegisterButton_Click
        pnlChoice.Controls.Add(btnShowRegister)

        ' View 2: username and password (hidden at start)
        pnlLogin = NewCard()
        pnlLogin.Controls.Add(NewLabel("Trader Login", 20, True))

        pnlLogin.Controls.Add(NewLabel("Username:"))
        txtTraderUser = NewTextBox()
        pnlLogin.Controls.Add(txtTraderUser)

        pnlLogin.Controls.Add(NewLabel("Password:"))
        txtTraderPass = NewTextBox(True)
        pnlLogin.Controls.Add(txtTraderPass)
        Dim lnkForgot As New LinkLabel()
        lnkForgot.Text = "Forgot password?"
        lnkForgot.Font = New Font("Segoe UI", 10)
        lnkForgot.AutoSize = True
        lnkForgot.Margin = New Padding(3, 8, 3, 0)
        AddHandler lnkForgot.LinkClicked, AddressOf Forgot_Click
        pnlLogin.Controls.Add(lnkForgot)


        Dim btnTraderLogin As Button = NewButton("Login", blue)
        btnTraderLogin.Margin = New Padding(3, 20, 3, 3)
        AddHandler btnTraderLogin.Click, AddressOf TraderLogin_Click
        pnlLogin.Controls.Add(btnTraderLogin)

        Dim btnBack As Button = NewButton("Back", grey)
        btnBack.Margin = New Padding(3, 10, 3, 3)
        AddHandler btnBack.Click, AddressOf ShowChoiceView
        pnlLogin.Controls.Add(btnBack)

        pnlLogin.Visible = False

        ' Both views share the same spot
        Dim holder As New FlowLayoutPanel()
        holder.FlowDirection = FlowDirection.TopDown
        holder.WrapContents = False
        holder.AutoSize = True
        holder.AutoSizeMode = AutoSizeMode.GrowAndShrink
        holder.Controls.Add(pnlChoice)
        holder.Controls.Add(pnlLogin)

        Return CenterCard(holder, Color.FromArgb(240, 244, 250))
    End Function

    ' ================= SWITCHING VIEWS =================
    Private Sub ShowLoginView(sender As Object, e As EventArgs)
        pnlChoice.Visible = False
        pnlLogin.Visible = True
        txtTraderUser.Focus()
    End Sub

    Private Sub ShowChoiceView(sender As Object, e As EventArgs)
        ResetScreen()
    End Sub

    Private Sub ResetScreen()
        pnlLogin.Visible = False
        pnlChoice.Visible = True
        txtTraderUser.Clear()
        txtTraderPass.Clear()
        txtAdminUser.Clear()
        txtAdminPass.Clear()
    End Sub

    ' ================= REGISTER =================
    Private Sub RegisterButton_Click(sender As Object, e As EventArgs)
        OpenRegisterForm()
    End Sub

    ' Stage 4 replaces the body of this routine
    Private Sub OpenRegisterForm()
        Dim reg As New frmRegister()
        Me.Hide()
        reg.ShowDialog()
        Me.Show()
        ResetScreen()
    End Sub


    ' ================= FORGOT PASSWORD =================
    Private Sub Forgot_Click(sender As Object, e As LinkLabelLinkClickedEventArgs)
        Using f As New frmForgotPassword()
            f.ShowDialog(Me)
        End Using
    End Sub

    ' ================= TRADER LOGIN =================
    Private Sub TraderLogin_Click(sender As Object, e As EventArgs)
        If txtTraderUser.Text.Trim() = "" OrElse txtTraderPass.Text = "" Then
            MessageBox.Show("Please enter your username and password.", "Missing information",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT TraderID, FullName, BusinessType, Phone_Number, House_Address, Gender " &
                    "FROM Traders WHERE Username = @u AND PasswordHash = @p"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@u", txtTraderUser.Text.Trim())
                    cmd.Parameters.AddWithValue("@p", HashPassword(txtTraderPass.Text))
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            CurrentTraderID = reader.GetInt32(0)
                            CurrentFullName = reader.GetString(1)
                            CurrentBusinessType = reader.GetString(2)
                            CurrentPhone = reader.GetString(3).Trim()
                            CurrentAddress = reader.GetString(4)
                            CurrentGender = reader.GetString(5)
                            CurrentUsername = txtTraderUser.Text.Trim()
                        Else
                            MessageBox.Show("Incorrect username or password.", "Login failed",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Return
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Login failed: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        OpenTraderDashboard()
    End Sub

    ' Stage 5 replaces the body of this routine
    Private Sub OpenTraderDashboard()
        Dim dash As New frmTrader()
        AddHandler dash.FormClosed, Sub(s, args)
                                        Me.Show()
                                        ResetScreen()
                                    End Sub
        Me.Hide()
        dash.Show()
    End Sub

    ' ================= ADMIN LOGIN =================
    Private Sub AdminLogin_Click(sender As Object, e As EventArgs)
        If txtAdminUser.Text.Trim() = "" OrElse txtAdminPass.Text = "" Then
            MessageBox.Show("Please enter the admin username and password.", "Missing information",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String = "SELECT COUNT(*) FROM Admins WHERE Username = @u AND PasswordHash = @p"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@u", txtAdminUser.Text.Trim())
                    cmd.Parameters.AddWithValue("@p", HashPassword(txtAdminPass.Text))
                    If Convert.ToInt32(cmd.ExecuteScalar()) = 0 Then
                        MessageBox.Show("Incorrect admin username or password.", "Login failed",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Login failed: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        CurrentAdminUsername = txtAdminUser.Text.Trim()
        OpenAdminDashboard()
    End Sub

    ' Stage 9 replaces the body of this routine
    Private Sub OpenAdminDashboard()
        Dim dash As New frmAdmin()
        AddHandler dash.FormClosed, Sub(s, args)
                                        Me.Show()
                                        ResetScreen()
                                    End Sub
        Me.Hide()
        dash.Show()
    End Sub

End Class