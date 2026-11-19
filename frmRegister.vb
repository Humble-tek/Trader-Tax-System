Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class frmRegister
    Public Sub New()
        InitializeComponent()
        ' Show a preview of the screen in the Visual Studio Design view only
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            BuildScreen()
        End If
    End Sub

    Private txtFullName As TextBox
    Private txtBusinessType As TextBox
    Private txtPhone As TextBox
    Private txtAddress As TextBox
    Private cmbGender As ComboBox
    Private txtUsername As TextBox
    Private txtPassword As TextBox
    Private txtConfirm As TextBox
    Private cmbQuestion As ComboBox
    Private txtAnswer As TextBox

    ' ================= STARTUP =================
    Private Sub frmRegister_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Trader Registration"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(560, 480)
        Me.Size = New Size(700, 820)
        Me.BackColor = Color.White
        BuildScreen()
    End Sub

    ' ================= SCREEN LAYOUT =================
    Private Sub BuildScreen()
        Dim host As New Panel()
        host.Dock = DockStyle.Fill
        host.AutoScroll = True
        host.AutoScrollMinSize = New Size(0, 940)
        host.BackColor = Color.FromArgb(240, 244, 250)
        Me.Controls.Add(host)

        Dim grid As New TableLayoutPanel()
        grid.Dock = DockStyle.Fill
        grid.ColumnCount = 1
        grid.RowCount = 3
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        grid.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        grid.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
        host.Controls.Add(grid)

        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.AutoSizeMode = AutoSizeMode.GrowAndShrink
        card.Padding = New Padding(20)
        card.Anchor = AnchorStyles.None
        grid.Controls.Add(card, 0, 1)

        Dim flag As Panel = NewGhanaFlag(90)
        flag.Margin = New Padding(3, 3, 3, 8)
        card.Controls.Add(flag)
        card.Controls.Add(NewLabel("Create Trader Account", 20, True))

        card.Controls.Add(NewLabel("Full Name:"))
        txtFullName = NewTextBox()
        card.Controls.Add(txtFullName)

        card.Controls.Add(NewLabel("Business Type:"))
        txtBusinessType = NewTextBox()
        card.Controls.Add(txtBusinessType)

        card.Controls.Add(NewLabel("Phone Number (used for payments):"))
        txtPhone = NewTextBox()
        txtPhone.MaxLength = 10
        card.Controls.Add(txtPhone)

        card.Controls.Add(NewLabel("House Address:"))
        txtAddress = NewTextBox()
        card.Controls.Add(txtAddress)

        card.Controls.Add(NewLabel("Gender:"))
        cmbGender = New ComboBox()
        cmbGender.DropDownStyle = ComboBoxStyle.DropDownList
        cmbGender.Font = New Font("Segoe UI", 11)
        cmbGender.Width = 280
        cmbGender.Items.Add("Male")
        cmbGender.Items.Add("Female")
        card.Controls.Add(cmbGender)

        card.Controls.Add(NewLabel("Username:"))
        txtUsername = NewTextBox()
        card.Controls.Add(txtUsername)

        card.Controls.Add(NewLabel("Password:"))
        txtPassword = NewTextBox(True)
        card.Controls.Add(txtPassword)

        card.Controls.Add(NewLabel("Confirm Password:"))
        txtConfirm = NewTextBox(True)
        card.Controls.Add(txtConfirm)

        card.Controls.Add(NewLabel("Security Question (for password recovery):"))
        cmbQuestion = New ComboBox()
        cmbQuestion.DropDownStyle = ComboBoxStyle.DropDownList
        cmbQuestion.Font = New Font("Segoe UI", 11)
        cmbQuestion.Width = 330
        cmbQuestion.Items.Add("What is the name of your first school?")
        cmbQuestion.Items.Add("What is your mother's maiden name?")
        cmbQuestion.Items.Add("What was the name of your first pet?")
        cmbQuestion.Items.Add("In which town were you born?")
        cmbQuestion.Items.Add("What is your favourite food?")
        card.Controls.Add(cmbQuestion)

        card.Controls.Add(NewLabel("Your Answer:"))
        txtAnswer = NewTextBox()
        txtAnswer.Width = 330
        card.Controls.Add(txtAnswer)

        Dim lblNote As Label = NewLabel("Remember this answer. You will need it to reset a forgotten password.", 9)
        lblNote.ForeColor = Color.Gray
        lblNote.MaximumSize = New Size(330, 0)
        card.Controls.Add(lblNote)

        Dim btnRegister As Button = NewButton("Register")
        btnRegister.Margin = New Padding(3, 20, 3, 3)
        AddHandler btnRegister.Click, AddressOf Register_Click
        card.Controls.Add(btnRegister)

        card.Controls.Add(NewLabel("Already registered?"))

        Dim btnLogin As Button = NewButton("Login", Color.FromArgb(30, 70, 140))
        AddHandler btnLogin.Click, AddressOf Login_Click
        card.Controls.Add(btnLogin)
    End Sub

    ' ================= LOGIN BUTTON: back to the Welcome page =================
    Private Sub Login_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    ' ================= REGISTER BUTTON =================
    Private Sub Register_Click(sender As Object, e As EventArgs)

        ' 1. All fields must be filled
        If txtFullName.Text.Trim() = "" OrElse
           txtBusinessType.Text.Trim() = "" OrElse
           txtPhone.Text.Trim() = "" OrElse
           txtAddress.Text.Trim() = "" OrElse
           cmbGender.SelectedIndex = -1 OrElse
           txtUsername.Text.Trim() = "" OrElse
           txtPassword.Text = "" OrElse
           cmbQuestion.SelectedIndex = -1 OrElse
           txtAnswer.Text.Trim() = "" Then
            MessageBox.Show("Please fill in all the fields.", "Missing information",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 2. Phone number format
        If Not IsValidPhone(txtPhone.Text.Trim()) Then
            MessageBox.Show("Enter a valid 10-digit phone number starting with 0, for example 0241234567.",
                            "Invalid phone number", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 3. Username rules
        If txtUsername.Text.Trim().Length < 4 OrElse txtUsername.Text.Contains(" ") Then
            MessageBox.Show("The username must have at least 4 characters and no spaces.",
                            "Invalid username", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 4. Password rules
        If txtPassword.Text.Length < 6 Then
            MessageBox.Show("The password must have at least 6 characters.", "Weak password",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtPassword.Text <> txtConfirm.Text Then
            MessageBox.Show("The passwords do not match.", "Password mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 5. Security answer rule
        If txtAnswer.Text.Trim().Length < 2 Then
            MessageBox.Show("The security answer must have at least 2 characters.", "Short answer",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()

                ' 6. Username must be unique
                Using checkUser As New SqlCommand("SELECT COUNT(*) FROM Traders WHERE Username = @u", conn)
                    checkUser.Parameters.AddWithValue("@u", txtUsername.Text.Trim())
                    If Convert.ToInt32(checkUser.ExecuteScalar()) > 0 Then
                        MessageBox.Show("That username is already taken. Please choose another.",
                                        "Username taken", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                End Using

                ' 7. Phone number must be unique
                Using checkPhone As New SqlCommand("SELECT COUNT(*) FROM Traders WHERE Phone_Number = @p", conn)
                    checkPhone.Parameters.AddWithValue("@p", txtPhone.Text.Trim())
                    If Convert.ToInt32(checkPhone.ExecuteScalar()) > 0 Then
                        MessageBox.Show("That phone number is already registered.",
                                        "Phone number taken", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                End Using

                ' 8. Save the trader
                Dim sql As String =
                    "INSERT INTO Traders (FullName, BusinessType, Username, PasswordHash, Phone_Number, House_Address, Gender, " &
                    "SecurityQuestion, SecurityAnswerHash) " &
                    "VALUES (@name, @type, @user, @hash, @phone, @address, @gender, @question, @answer)"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@name", txtFullName.Text.Trim())
                    cmd.Parameters.AddWithValue("@type", txtBusinessType.Text.Trim())
                    cmd.Parameters.AddWithValue("@user", txtUsername.Text.Trim())
                    cmd.Parameters.AddWithValue("@hash", HashPassword(txtPassword.Text))
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim())
                    cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim())
                    cmd.Parameters.AddWithValue("@gender", cmbGender.SelectedItem.ToString())
                    cmd.Parameters.AddWithValue("@question", cmbQuestion.SelectedItem.ToString())
                    cmd.Parameters.AddWithValue("@answer", HashPassword(txtAnswer.Text.Trim().ToLowerInvariant()))
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Registration successful! You can now log in.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Registration failed: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class