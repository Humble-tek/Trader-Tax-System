Imports System.Security.Cryptography
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucAdminTraders

    Private grid As DataGridView
    Private txtSearch As TextBox
    Private lblCount As Label
    Private table As DataTable
    Private view As DataView

    ' ================= BUILD THE PAGE =================
    Private Sub ucAdminTraders_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 4
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        Me.Controls.Add(layout)

        layout.Controls.Add(NewLabel("Registered Traders", 22, True), 0, 0)

        ' ---------- Search row ----------
        Dim searchRow As New FlowLayoutPanel()
        searchRow.FlowDirection = FlowDirection.LeftToRight
        searchRow.AutoSize = True
        searchRow.Controls.Add(NewLabel("Search (name, username, phone or business):"))

        txtSearch = NewTextBox()
        txtSearch.Width = 260
        AddHandler txtSearch.TextChanged, AddressOf Search_Changed
        searchRow.Controls.Add(txtSearch)

        Dim btnRefresh As Button = NewButton("Refresh", Color.FromArgb(110, 110, 110))
        btnRefresh.Width = 110
        btnRefresh.Height = 32
        btnRefresh.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnRefresh.Margin = New Padding(10, 5, 3, 3)
        AddHandler btnRefresh.Click, Sub(s, args) LoadData()
        searchRow.Controls.Add(btnRefresh)
        Dim btnReset As Button = NewButton("Reset Password", DangerColor)
        btnReset.Width = 160
        btnReset.Height = 32
        btnReset.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnReset.Margin = New Padding(10, 5, 3, 3)
        AddHandler btnReset.Click, AddressOf ResetPassword_Click
        searchRow.Controls.Add(btnReset)
        layout.Controls.Add(searchRow, 0, 1)

        lblCount = NewLabel("", 11, True)
        layout.Controls.Add(lblCount, 0, 2)

        ' ---------- Table ----------
        grid = New DataGridView()
        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = False
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.Font = New Font("Segoe UI", 10)
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        grid.ColumnHeadersHeight = 38
        grid.RowTemplate.Height = 32
        layout.Controls.Add(grid, 0, 3)

        LoadData()
    End Sub

    ' ================= LOAD ALL TRADERS =================
    Private Sub LoadData()
        table = New DataTable()
        table.Columns.Add("ID", GetType(Integer))
        table.Columns.Add("Full Name", GetType(String))
        table.Columns.Add("Business Type", GetType(String))
        table.Columns.Add("Phone", GetType(String))
        table.Columns.Add("Address", GetType(String))
        table.Columns.Add("Gender", GetType(String))
        table.Columns.Add("Username", GetType(String))
        table.Columns.Add("Payments", GetType(Integer))
        table.Columns.Add("Total Paid (GHS)", GetType(Decimal))

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT t.TraderID, t.FullName, t.BusinessType, t.Phone_Number, t.House_Address, t.Gender, t.Username, " &
                    "COUNT(p.PaymentID), ISNULL(SUM(p.Amount), 0) " &
                    "FROM Traders t LEFT JOIN Payments p ON p.TraderID = t.TraderID " &
                    "GROUP BY t.TraderID, t.FullName, t.BusinessType, t.Phone_Number, t.House_Address, t.Gender, t.Username " &
                    "ORDER BY t.FullName"
                Using cmd As New SqlCommand(sql, conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            table.Rows.Add(reader.GetInt32(0),
                                           reader.GetString(1),
                                           reader.GetString(2),
                                           reader.GetString(3).Trim(),
                                           reader.GetString(4),
                                           reader.GetString(5),
                                           reader.GetString(6),
                                           reader.GetInt32(7),
                                           reader.GetDecimal(8))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load the traders: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        view = New DataView(table)
        grid.DataSource = view

        grid.Columns("ID").FillWeight = 8
        grid.Columns("Full Name").FillWeight = 22
        grid.Columns("Business Type").FillWeight = 16
        grid.Columns("Phone").FillWeight = 14
        grid.Columns("Address").FillWeight = 18
        grid.Columns("Gender").FillWeight = 10
        grid.Columns("Username").FillWeight = 14
        grid.Columns("Payments").FillWeight = 11
        grid.Columns("Total Paid (GHS)").FillWeight = 15
        grid.Columns("Total Paid (GHS)").DefaultCellStyle.Format = "N2"
        grid.Columns("Total Paid (GHS)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        grid.Columns("Payments").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

        ApplyFilterNow()
    End Sub

    ' ================= SEARCH =================
    Private Sub Search_Changed(sender As Object, e As EventArgs)
        ApplyFilterNow()
    End Sub

    Private Sub ApplyFilterNow()
        If view Is Nothing Then Return

        Dim s As String = txtSearch.Text.Trim()
        If s = "" Then
            view.RowFilter = ""
        Else
            Dim p As String = EscapeLike(s)
            view.RowFilter = "[Full Name] LIKE '%" & p & "%' OR [Username] LIKE '%" & p &
                             "%' OR [Phone] LIKE '%" & p & "%' OR [Business Type] LIKE '%" & p & "%'"
        End If

        Dim total As Decimal = 0D
        For Each rv As DataRowView In view
            total += Convert.ToDecimal(rv("Total Paid (GHS)"))
        Next

        lblCount.Text = "Showing " & view.Count & " of " & table.Rows.Count &
                        " registered trader(s)   |   Total paid by these traders: " & FormatMoney(total)
    End Sub

    ' ================= ADMIN: RESET A TRADER'S PASSWORD =================
    Private Function GenerateTempPassword() As String
        Dim chars As String = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789"
        Dim result As New StringBuilder()
        For i As Integer = 1 To 8
            result.Append(chars(RandomNumberGenerator.GetInt32(chars.Length)))
        Next
        Return result.ToString()
    End Function

    Private Sub ResetPassword_Click(sender As Object, e As EventArgs)
        If grid.CurrentRow Is Nothing OrElse grid.CurrentRow.Cells("ID").Value Is Nothing Then
            MessageBox.Show("Select a trader in the table first.", "No trader selected",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim id As Integer = Convert.ToInt32(grid.CurrentRow.Cells("ID").Value)
        Dim traderName As String = Convert.ToString(grid.CurrentRow.Cells("Full Name").Value)

        If MessageBox.Show("Reset the password of " & traderName & "?" & vbCrLf &
                           "A temporary password will be created.", "Reset password",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Dim temp As String = GenerateTempPassword()

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Using cmd As New SqlCommand("UPDATE Traders SET PasswordHash = @p WHERE TraderID = @id", conn)
                    cmd.Parameters.AddWithValue("@p", HashPassword(temp))
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("The temporary password for " & traderName & " is:" & vbCrLf & vbCrLf & temp & vbCrLf & vbCrLf &
                            "Give it to the trader privately. It is shown only once.",
                            "Password reset", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("Could not reset the password: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function EscapeLike(text As String) As String
        Dim sb As New StringBuilder()
        For Each ch As Char In text
            Select Case ch
                Case "'"c
                    sb.Append("''")
                Case "["c, "]"c, "*"c, "%"c
                    sb.Append("[").Append(ch).Append("]")
                Case Else
                    sb.Append(ch)
            End Select
        Next
        Return sb.ToString()
    End Function

End Class