Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucTaxHistory

    Private grid As DataGridView
    Private lblTotal As Label
    Private lblEmpty As Label

    Private ReadOnly monthNames As String() = {"January", "February", "March", "April", "May", "June",
                                               "July", "August", "September", "October", "November", "December"}

    ' ================= BUILD THE PAGE =================
    Private Sub ucTaxHistory_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 4
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Me.Controls.Add(layout)

        layout.Controls.Add(NewLabel("Tax History", 22, True), 0, 0)

        lblTotal = NewLabel("Total paid: GHS 0.00", 13, True)
        layout.Controls.Add(lblTotal, 0, 1)

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
        layout.Controls.Add(grid, 0, 2)

        Dim bottom As New FlowLayoutPanel()
        bottom.FlowDirection = FlowDirection.LeftToRight
        bottom.AutoSize = True

        Dim btnReceipt As Button = NewButton("View Receipt", Color.FromArgb(30, 70, 140))
        btnReceipt.Margin = New Padding(3, 12, 3, 3)
        AddHandler btnReceipt.Click, AddressOf ViewReceipt_Click
        bottom.Controls.Add(btnReceipt)

        Dim btnRefresh As Button = NewButton("Refresh", Color.FromArgb(110, 110, 110))
        btnRefresh.Margin = New Padding(10, 12, 3, 3)
        AddHandler btnRefresh.Click, Sub(s, args) LoadData()
        bottom.Controls.Add(btnRefresh)

        layout.Controls.Add(bottom, 0, 3)

        lblEmpty = NewLabel("You have not made any payments yet.", 12)
        lblEmpty.ForeColor = Color.Gray
        lblEmpty.Visible = False
        layout.Controls.Add(lblEmpty, 0, 2)

        LoadData()
    End Sub

    ' ================= LOAD THE PAYMENTS =================
    Private Sub LoadData()
        Dim table As New DataTable()
        table.Columns.Add("Receipt No", GetType(String))
        table.Columns.Add("Date", GetType(String))
        table.Columns.Add("Tax Type", GetType(String))
        table.Columns.Add("Period", GetType(String))
        table.Columns.Add("Income (GHS)", GetType(Decimal))
        table.Columns.Add("Rate (%)", GetType(Decimal))
        table.Columns.Add("Amount Paid (GHS)", GetType(Decimal))

        Dim total As Decimal = 0D

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT Reference, PaymentDate, TaxType, PayMonth, PayYear, Income, Rate, Amount " &
                    "FROM Payments WHERE TraderID = @t ORDER BY PaymentDate DESC"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@t", CurrentTraderID)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim period As String = monthNames(reader.GetInt32(3) - 1) & " " & reader.GetInt32(4)
                            Dim amount As Decimal = reader.GetDecimal(7)
                            total += amount
                            table.Rows.Add(reader.GetString(0),
                                           reader.GetDateTime(1).ToString("dd MMM yyyy HH:mm"),
                                           reader.GetString(2),
                                           period,
                                           reader.GetDecimal(5),
                                           reader.GetDecimal(6),
                                           amount)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load your tax history: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        grid.DataSource = table
        grid.Columns("Income (GHS)").DefaultCellStyle.Format = "N2"
        grid.Columns("Rate (%)").DefaultCellStyle.Format = "0.##"
        grid.Columns("Amount Paid (GHS)").DefaultCellStyle.Format = "N2"
        grid.Columns("Income (GHS)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        grid.Columns("Rate (%)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        grid.Columns("Amount Paid (GHS)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

        lblTotal.Text = "Total paid: " & FormatMoney(total) & "   (" & table.Rows.Count & " payment(s))"
        lblEmpty.Visible = (table.Rows.Count = 0)
        grid.Visible = (table.Rows.Count > 0)
    End Sub

    ' ================= REOPEN A RECEIPT =================
    Private Sub ViewReceipt_Click(sender As Object, e As EventArgs)
        If grid.CurrentRow Is Nothing Then
            MessageBox.Show("Select a payment in the table first.", "No payment selected",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim reference As String = grid.CurrentRow.Cells("Receipt No").Value.ToString()

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT PaymentDate, TaxType, PayMonth, PayYear, Income, Rate, Amount, Phone_Number " &
                    "FROM Payments WHERE Reference = @r AND TraderID = @t"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@r", reference)
                    cmd.Parameters.AddWithValue("@t", CurrentTraderID)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim period As String = monthNames(reader.GetInt32(2) - 1) & " " & reader.GetInt32(3)
                            Using receipt As New frmReceipt(reference, reader.GetDateTime(0), CurrentFullName,
                                                            reader.GetString(1), period, reader.GetDecimal(4),
                                                            reader.GetDecimal(5), reader.GetDecimal(6),
                                                            reader.GetString(7).Trim())
                                receipt.ShowDialog(Me.FindForm())
                            End Using
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not open the receipt: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class