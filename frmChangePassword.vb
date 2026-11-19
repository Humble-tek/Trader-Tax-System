Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class frmChangePassword

    Private ReadOnly forAdmin As Boolean
    Private txtCurrent As TextBox
    Private txtNew As TextBox
    Private txtConfirm As TextBox

    Public Sub New(isAdmin As Boolean)
        InitializeComponent()
        forAdmin = isAdmin
    End Sub

    ' ================= STARTUP =================
    Private Sub frmChangePassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Change Password"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.Size = New Size(440, 580)
        Me.BackColor = Color.White

        Dim card As New FlowLayoutPanel()
        card.Dock = DockStyle.Fill
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoScroll = True
        card.Padding = New Padding(30, 20, 20, 20)
        Me.Controls.Add(card)

        card.Controls.Add(NewLabel("Change Password", 20, True))
        If forAdmin Then
            card.Controls.Add(NewLabel("Administrator account", 11))
        Else
            card.Controls.Add(NewLabel("Account: " & CurrentUsername, 11))
        End If

        card.Controls.Add(NewLabel("Current password:"))
        txtCurrent = NewTextBox(True)
        card.Controls.Add(txtCurrent)

        card.Controls.Add(NewLabel("New password:"))
        txtNew = NewTextBox(True)
        card.Controls.Add(txtNew)

        card.Controls.Add(NewLabel("Confirm new password:"))
        txtConfirm = NewTextBox(True)
        card.Controls.Add(txtConfirm)

        Dim lblNote As Label = NewLabel("The new password must have at least 6 characters.", 9)
        lblNote.ForeColor = Color.Gray
        card.Controls.Add(lblNote)

        Dim btnSave As Button = NewButton("Change Password")
        btnSave.Margin = New Padding(3, 20, 3, 3)
        AddHandler btnSave.Click, AddressOf Save_Click
        card.Controls.Add(btnSave)

        Dim btnCancel As Button = NewButton("Cancel", Color.FromArgb(110, 110, 110))
        btnCancel.Margin = New Padding(3, 10, 3, 3)
        AddHandler btnCancel.Click, Sub(s, args) Me.Close()
        card.Controls.Add(btnCancel)
        Me.CancelButton = btnCancel
    End Sub

    ' ================= SAVE =================
    Private Sub Save_Click(sender As Object, e As EventArgs)

        If txtCurrent.Text = "" OrElse txtNew.Text = "" OrElse txtConfirm.Text = "" Then
            MessageBox.Show("Please fill in all the boxes.", "Missing information",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtNew.Text.Length < 6 Then
            MessageBox.Show("The new password must have at least 6 characters.", "Weak password",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtNew.Text <> txtConfirm.Text Then
            MessageBox.Show("The new passwords do not match.", "Password mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If txtNew.Text = txtCurrent.Text Then
            MessageBox.Show("The new password must be different from the current password.", "Same password",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim selectSql As String
        Dim updateSql As String
        Dim keyValue As Object

        If forAdmin Then
            selectSql = "SELECT PasswordHash FROM Admins WHERE Username = @key"
            updateSql = "UPDATE Admins SET PasswordHash = @p WHERE Username = @key"
            keyValue = CurrentAdminUsername
        Else
            selectSql = "SELECT PasswordHash FROM Traders WHERE TraderID = @key"
            updateSql = "UPDATE Traders SET PasswordHash = @p WHERE TraderID = @key"
            keyValue = CurrentTraderID
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()

                Dim storedHash As Object
                Using cmd As New SqlCommand(selectSql, conn)
                    cmd.Parameters.AddWithValue("@key", keyValue)
                    storedHash = cmd.ExecuteScalar()
                End Using

                If storedHash Is Nothing OrElse IsDBNull(storedHash) Then
                    MessageBox.Show("Your account could not be found. Please log in again.", "Account not found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                If Convert.ToString(storedHash) <> HashPassword(txtCurrent.Text) Then
                    MessageBox.Show("The current password is not correct.", "Wrong password",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                Using cmd As New SqlCommand(updateSql, conn)
                    cmd.Parameters.AddWithValue("@p", HashPassword(txtNew.Text))
                    cmd.Parameters.AddWithValue("@key", keyValue)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Your password has been changed. Use the new password the next time you log in.",
                            "Password changed", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()

        Catch ex As Exception
            MessageBox.Show("Could not change the password: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class