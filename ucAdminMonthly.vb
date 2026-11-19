Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucAdminMonthly

    Private cmbType As ComboBox
    Private cmbYear As ComboBox
    Private grid As DataGridView
    Private lblSummary As Label

    Private ReadOnly monthShort As String() = {"Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}
    Private ReadOnly taxTypes As String() = {"Income Tax", "Value Added Tax", "Business Operating Permit"}

    Private ReadOnly PaidText As String = ChrW(&H2714) & " Paid"
    Private ReadOnly UnpaidText As String = ChrW(&H2718) & " Not Paid"
    Private ReadOnly NotDueText As String = ChrW(&H2014).ToString()

    ' ================= BUILD THE PAGE =================
    Private Sub ucAdminMonthly_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

        layout.Controls.Add(NewLabel("Monthly Payments", 22, True), 0, 0)

        ' ---------- Filters ----------
        Dim filters As New FlowLayoutPanel()
        filters.FlowDirection = FlowDirection.LeftToRight
        filters.AutoSize = True

        filters.Controls.Add(NewLabel("Tax Type:"))
        cmbType = MakeCombo(240)
        For Each t As String In taxTypes
            cmbType.Items.Add(t)
        Next
        cmbType.SelectedIndex = 0
        filters.Controls.Add(cmbType)

        filters.Controls.Add(NewLabel("Year:"))
        cmbYear = MakeCombo(100)
        cmbYear.Items.Add((DateTime.Now.Year - 1).ToString())
        cmbYear.Items.Add(DateTime.Now.Year.ToString())
        cmbYear.SelectedIndex = 1
        filters.Controls.Add(cmbYear)

        Dim btnRefresh As Button = NewButton("Refresh", Color.FromArgb(110, 110, 110))
        btnRefresh.Width = 110
        btnRefresh.Height = 32
        btnRefresh.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnRefresh.Margin = New Padding(15, 5, 3, 3)
        AddHandler btnRefresh.Click, Sub(s, args) LoadData()
        filters.Controls.Add(btnRefresh)

        layout.Controls.Add(filters, 0, 1)

        lblSummary = NewLabel("", 11, True)
        layout.Controls.Add(lblSummary, 0, 2)

        ' ---------- Table ----------
        grid = New DataGridView()
        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.CellSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.Font = New Font("Segoe UI", 10)
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        grid.ColumnHeadersHeight = 38
        grid.RowTemplate.Height = 34
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        AddHandler grid.DataBindingComplete, AddressOf ColourCells
        layout.Controls.Add(grid, 0, 3)

        AddHandler cmbType.SelectedIndexChanged, Sub(s, args) LoadData()
        AddHandler cmbYear.SelectedIndexChanged, Sub(s, args) LoadData()

        LoadData()
    End Sub

    Private Function MakeCombo(width As Integer) As ComboBox
        Dim c As New ComboBox()
        c.DropDownStyle = ComboBoxStyle.DropDownList
        c.Font = New Font("Segoe UI", 11)
        c.Width = width
        c.Margin = New Padding(3, 5, 15, 3)
        Return c
    End Function

    ' ================= WORK OUT PAID AND NOT PAID =================
    Private Sub LoadData()
        If cmbType.SelectedItem Is Nothing OrElse cmbYear.SelectedItem Is Nothing Then Return

        Dim taxType As String = cmbType.SelectedItem.ToString()
        Dim yearNo As Integer = Integer.Parse(cmbYear.SelectedItem.ToString())

        ' Months that are already due in the chosen year
        Dim dueMonths As Integer
        If yearNo < DateTime.Now.Year Then
            dueMonths = 12
        ElseIf yearNo = DateTime.Now.Year Then
            dueMonths = DateTime.Now.Month
        Else
            dueMonths = 0
        End If

        Dim traders As New List(Of KeyValuePair(Of Integer, String))
        Dim paid As New HashSet(Of String)

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()

                Using cmd As New SqlCommand("SELECT TraderID, FullName FROM Traders ORDER BY FullName", conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            traders.Add(New KeyValuePair(Of Integer, String)(reader.GetInt32(0), reader.GetString(1)))
                        End While
                    End Using
                End Using

                Dim sql As String =
                    "SELECT TraderID, PayMonth FROM Payments WHERE TaxType = @type AND PayYear = @y"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@type", taxType)
                    cmd.Parameters.AddWithValue("@y", yearNo)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            paid.Add(reader.GetInt32(0) & "-" & reader.GetInt32(1))
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load the monthly payments: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        ' Build the table: Trader, 12 months, two totals
        Dim table As New DataTable()
        table.Columns.Add("Trader", GetType(String))
        For Each m As String In monthShort
            table.Columns.Add(m, GetType(String))
        Next
        table.Columns.Add("Months Paid", GetType(Integer))
        table.Columns.Add("Months Not Paid", GetType(Integer))

        Dim totalPaid As Integer = 0
        Dim totalNotPaid As Integer = 0

        For Each t As KeyValuePair(Of Integer, String) In traders
            Dim values As New List(Of Object)
            values.Add(t.Value)
            Dim paidCount As Integer = 0
            Dim notPaidCount As Integer = 0

            For m As Integer = 1 To 12
                If paid.Contains(t.Key & "-" & m) Then
                    values.Add(PaidText)
                    paidCount += 1
                ElseIf m <= dueMonths Then
                    values.Add(UnpaidText)
                    notPaidCount += 1
                Else
                    values.Add(NotDueText)
                End If
            Next

            values.Add(paidCount)
            values.Add(notPaidCount)
            table.Rows.Add(values.ToArray())
            totalPaid += paidCount
            totalNotPaid += notPaidCount
        Next

        grid.DataSource = table

        ' Column layout
        grid.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        grid.Columns(0).Width = 200
        grid.Columns(0).Frozen = True
        grid.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        For i As Integer = 0 To grid.Columns.Count - 1
            grid.Columns(i).SortMode = DataGridViewColumnSortMode.NotSortable
            If i >= 1 AndAlso i <= 12 Then grid.Columns(i).MinimumWidth = 95
            If i > 12 Then grid.Columns(i).MinimumWidth = 110
        Next

        lblSummary.Text = taxType & ", " & yearNo & "   |   Traders: " & traders.Count &
                          "   |   Paid: " & totalPaid & "   |   Not paid: " & totalNotPaid &
                          "   |   " & NotDueText & " = month not yet due"
    End Sub

    ' ================= COLOURS: GREEN PAID, RED NOT PAID =================
    Private Sub ColourCells(sender As Object, e As DataGridViewBindingCompleteEventArgs)
        For Each row As DataGridViewRow In grid.Rows
            For i As Integer = 1 To 12
                Dim cell As DataGridViewCell = row.Cells(i)
                Dim text As String = Convert.ToString(cell.Value)
                If text.Contains("Not Paid") Then
                    cell.Style.BackColor = Color.FromArgb(255, 225, 225)
                    cell.Style.ForeColor = DangerColor
                    cell.Style.Font = New Font("Segoe UI", 10, FontStyle.Bold)
                ElseIf text.Contains("Paid") Then
                    cell.Style.BackColor = Color.FromArgb(220, 245, 228)
                    cell.Style.ForeColor = PrimaryColor
                    cell.Style.Font = New Font("Segoe UI", 10, FontStyle.Bold)
                Else
                    cell.Style.ForeColor = Color.Gray
                End If
            Next
        Next
    End Sub

End Class