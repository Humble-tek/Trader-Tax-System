Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class frmForgotPassword
    Public Sub New()
        InitializeComponent()
        ' Show a preview of the screen in the Visual Studio Design view only
        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            BuildScreen()
        End If
    End Sub

    ' Wrong-answer counters survive while the app is open
    Private Shared failedAttempts As New Dictionary(Of String, Integer)
    Private Shared lockedUntil As New Dictionary(Of String, DateTime)

    Private txtUsername As TextBox
    Private lblQuestion As Label
    Private txtAnswer As TextBox
    Private txtNew As TextBox
    Private txtConfirm As TextBox
    Private pnlStep1 As FlowLayoutPanel
    Private pnlStep2 As FlowLayoutPanel

    Private foundTraderId As Integer
    Private foundUsername As String = ""
    Private answerHashOnFile As String = ""

    Private Const MaxAttempts As Integer = 3
    Private Const LockMinutes As Integer = 10

    ' ================= STARTUP =================
    Private Sub frmForgotPassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Reset Password"
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MinimumSize = New Size(520, 480)
        Me.Size = New Size(560, 700)
        Me.BackColor = Color.White
        BuildScreen()
    End Sub

    ' ================= SCREEN LAYOUT =================
    Private Sub BuildScreen()
        Dim host As New Panel()
        host.Dock = DockStyle.Fill
        host.AutoScroll = True
        host.AutoScrollMinSize = New Size(0, 640)
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

        Dim flag As Panel = NewGhanaFlag(70)
        flag.Margin = New Padding(3, 3, 3, 8)
        card.Controls.Add(flag)
        card.Controls.Add(NewLabel("Reset Password", 20, True))

        ' ---------- Step 1: username ----------
        pnlStep1 = New FlowLayoutPanel()
        pnlStep1.FlowDirection = FlowDirection.TopDown
        pnlStep1.WrapContents = False
        pnlStep1.AutoSize = True

        pnlStep1.Controls.Add(NewLabel("Enter your username:"))
        txtUsername = NewTextBox()
        pnlStep1.Controls.Add(txtUsername)

        Dim btnNext As Button = NewButton("Next")
        btnNext.Margin = New Padding(3, 15, 3, 3)
        AddHandler btnNext.Click, AddressOf Next_Click
        pnlStep1.Controls.Add(btnNext)
        card.Controls.Add(pnlStep1)

        ' ---------- Step 2: answer and new password ----------
        pnlStep2 = New FlowLayoutPanel()
        pnlStep2.FlowDirection = FlowDirection.TopDown
        pnlStep2.WrapContents = False
        pnlStep2.AutoSize = True
        pnlStep2.Visible = False

        pnlStep2.Controls.Add(NewLabel("Your security question:", 10, True))
        lblQuestion = NewLabel("", 11)
        lblQuestion.MaximumSize = New Size(320, 0)
        pnlStep2.Controls.Add(lblQuestion)

        pnlStep2.Controls.Add(NewLabel("Your answer:"))
        txtAnswer = NewTextBox()
        pnlStep2.Controls.Add(txtAnswer)

        pnlStep2.Controls.Add(NewLabel("New password:"))
        txtNew = NewTextBox(True)
        pnlStep2.Controls.Add(txtNew)

        pnlStep2.Controls.Add(NewLabel("Confirm new password:"))
        txtConfirm = NewTextBox(True)
        pnlStep2.Controls.Add(txtConfirm)

        Dim btnReset As Button = NewButton("Reset Password")
        btnReset.Margin = New Padding(3, 15, 3, 3)
        AddHandler btnReset.Click, AddressOf Reset_Click
        pnlStep2.Controls.Add(btnReset)
        card.Controls.Add(pnlStep2)

        Dim btnCancel As Button = NewButton("Cancel", Color.FromArgb(110, 110, 110))
        btnCancel.Margin = New Padding(3, 10, 3, 3)
        AddHandler btnCancel.Click, Sub(s, args) Me.Close()
        card.Controls.Add(btnCancel)
    End Sub

    ' ================= LOCK AFTER TOO MANY WRONG ANSWERS =================
    Private Function IsLocked(user As String) As Boolean
        Dim key As String = user.ToLowerInvariant()
        If lockedUntil.ContainsKey(key) Then
            If lockedUntil(key) > DateTime.Now Then
                MessageBox.Show("Too many wrong answers. Please try again after " &
                                lockedUntil(key).ToString("HH:mm") & ", or ask the admin to reset your password.",
                                "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return True
            Else
                lockedUntil.Remove(key)
                failedAttempts.Remove(key)
            End If
        End If
        Return False
    End Function

    ' ================= STEP 1: FIND THE ACCOUNT =================
    Private Sub Next_Click(sender As Object, e As EventArgs)
        Dim user As String = txtUsername.Text.Trim()
        If user = "" Then
            MessageBox.Show("Please enter your username.", "Missing information",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If IsLocked(user) Then Return

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT TraderID, Username, SecurityQuestion, SecurityAnswerHash FROM Traders WHERE Username = @u"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@u", user)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        If Not reader.Read() Then
                            MessageBox.Show("No account was found with that username.", "Not found",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            Return
                        End If

                        If reader.IsDBNull(2) OrElse reader.IsDBNull(3) OrElse reader.GetString(2).Trim() = "" Then
                            MessageBox.Show("This account has no security question. Please ask the admin to reset your password.",
                                            "No security question", MessageBoxButtons.OK, MessageBoxIcon.Information)
                            Return
                        End If

                        foundTraderId = reader.GetInt32(0)
                        foundUsername = reader.GetString(1)
                        lblQuestion.Text = reader.GetString(2)
                        answerHashOnFile = reader.GetString(3)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not look up the account: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        pnlStep1.Visible = False
        pnlStep2.Visible = True
        txtAnswer.Focus()
    End Sub

    ' ================= STEP 2: CHECK THE ANSWER AND SAVE =================
    Private Sub Reset_Click(sender As Object, e As EventArgs)
        If IsLocked(foundUsername) Then
            Me.Close()
            Return
        End If

        If txtAnswer.Text.Trim() = "" Then
            MessageBox.Show("Please type your answer.", "Missing answer",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim key As String = foundUsername.ToLowerInvariant()

        If HashPassword(txtAnswer.Text.Trim().ToLowerInvariant()) <> answerHashOnFile Then
            Dim count As Integer = 0
            If failedAttempts.ContainsKey(key) Then count = failedAttempts(key)
            count += 1
            failedAttempts(key) = count

            If count >= MaxAttempts Then
                lockedUntil(key) = DateTime.Now.AddMinutes(LockMinutes)
                MessageBox.Show("Too many wrong answers. This reset is locked for " & LockMinutes &
                                " minutes. You can also ask the admin to reset your password.",
                                "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Me.Close()
            Else
                MessageBox.Show("That answer is not correct. You have " & (MaxAttempts - count) & " attempt(s) left.",
                                "Wrong answer", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return
        End If

        If txtNew.Text.Length < 6 Then
            MessageBox.Show("The new password must have at least 6 characters.", "Weak password",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtNew.Text <> txtConfirm.Text Then
            MessageBox.Show("The passwords do not match.", "Password mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Using cmd As New SqlCommand("UPDATE Traders SET PasswordHash = @p WHERE TraderID = @id", conn)
                    cmd.Parameters.AddWithValue("@p", HashPassword(txtNew.Text))
                    cmd.Parameters.AddWithValue("@id", foundTraderId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            failedAttempts.Remove(key)
            MessageBox.Show("Your password has been reset. You can now log in with your new password.",
                            "Password reset", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Could not reset the password: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class