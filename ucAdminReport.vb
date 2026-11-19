Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucAdminReport

    Private cmbType As ComboBox
    Private cmbYear As ComboBox
    Private cmbMonth As ComboBox
    Private cmbTrader As ComboBox
    Private grid As DataGridView
    Private lblTotal As Label
    Private lblDetails As Label
    Private lblSelected As Label
    Private loadingTraders As Boolean = False

    Private Class TraderItem
        Public Id As Integer
        Public Name As String
        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    Private currentTable As DataTable
    Private currentTotal As Decimal
    Private currentBreakdown As String = ""
    Private printLines As List(Of String)
    Private printIndex As Integer

    Private ReadOnly monthNames As String() = {"January", "February", "March", "April", "May", "June",
                                               "July", "August", "September", "October", "November", "December"}
    Private ReadOnly taxTypes As String() = {"Income Tax", "Value Added Tax", "Business Operating Permit"}

    ' ================= BUILD THE PAGE =================
    Private Sub ucAdminReport_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = True
        Me.AutoScrollMinSize = New Size(720, 640)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 5
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Me.Controls.Add(layout)

        layout.Controls.Add(NewLabel("Payment Report and Calculator", 22, True), 0, 0)

        ' ---------- Filters ----------
        Dim filters As New FlowLayoutPanel()
        filters.FlowDirection = FlowDirection.LeftToRight
        filters.WrapContents = True
        filters.AutoSize = True
        filters.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right

        filters.Controls.Add(NewLabel("Tax Type:"))
        cmbType = MakeCombo(190)
        cmbType.Items.Add("All tax types")
        For Each t As String In taxTypes
            cmbType.Items.Add(t)
        Next
        cmbType.SelectedIndex = 0
        filters.Controls.Add(cmbType)

        filters.Controls.Add(NewLabel("Year:"))
        cmbYear = MakeCombo(110)
        cmbYear.Items.Add("All years")
        cmbYear.Items.Add((DateTime.Now.Year - 1).ToString())
        cmbYear.Items.Add(DateTime.Now.Year.ToString())
        cmbYear.SelectedIndex = 0
        filters.Controls.Add(cmbYear)

        filters.Controls.Add(NewLabel("Month:"))
        cmbMonth = MakeCombo(130)
        cmbMonth.Items.Add("All months")
        For Each m As String In monthNames
            cmbMonth.Items.Add(m)
        Next
        cmbMonth.SelectedIndex = 0
        filters.Controls.Add(cmbMonth)

        filters.Controls.Add(NewLabel("Trader:"))
        cmbTrader = MakeCombo(200)
        FillTraders()
        filters.Controls.Add(cmbTrader)

        Dim btnRefresh As Button = SmallButton("Refresh", Color.FromArgb(110, 110, 110))
        AddHandler btnRefresh.Click, Sub(s, args)
                                         FillTraders()
                                         LoadData()
                                     End Sub
        filters.Controls.Add(btnRefresh)

        Dim btnPrint As Button = SmallButton("Print Report", Color.FromArgb(30, 70, 140))
        AddHandler btnPrint.Click, AddressOf Print_Click
        filters.Controls.Add(btnPrint)

        layout.Controls.Add(filters, 0, 1)

        ' ---------- Totals card ----------
        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True
        card.BackColor = Color.White
        card.Padding = New Padding(12)
        card.Margin = New Padding(3, 8, 3, 8)

        lblTotal = NewLabel("Total collected: GHS 0.00", 18, True)
        lblTotal.ForeColor = PrimaryColor
        card.Controls.Add(lblTotal)

        lblDetails = NewLabel("", 10)
        lblDetails.MaximumSize = New Size(900, 0)
        card.Controls.Add(lblDetails)
        layout.Controls.Add(card, 0, 2)

        ' ---------- Table ----------
        grid = New DataGridView()
        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.RowHeadersVisible = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect = True
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.Font = New Font("Segoe UI", 10)
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        grid.ColumnHeadersHeight = 36
        grid.RowTemplate.Height = 30
        AddHandler grid.SelectionChanged, AddressOf UpdateSelected
        layout.Controls.Add(grid, 0, 3)

        lblSelected = NewLabel("", 11, True)
        layout.Controls.Add(lblSelected, 0, 4)

        ' Filters react as soon as they change
        AddHandler cmbType.SelectedIndexChanged, Sub(s, args) LoadData()
        AddHandler cmbYear.SelectedIndexChanged, Sub(s, args) LoadData()
        AddHandler cmbMonth.SelectedIndexChanged, Sub(s, args) LoadData()
        AddHandler cmbTrader.SelectedIndexChanged, Sub(s, args) LoadData()

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

    Private Function SmallButton(text As String, background As Color) As Button
        Dim b As Button = NewButton(text, background)
        b.Width = 130
        b.Height = 32
        b.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        b.Margin = New Padding(3, 5, 8, 3)
        Return b
    End Function

    ' Fills the trader drop-down, keeping the current choice if possible
    ' Fills the trader drop-down, keeping the current choice if possible
    Private Sub FillTraders()
        loadingTraders = True

        Dim keepId As Integer = 0
        Dim chosen As TraderItem = TryCast(cmbTrader.SelectedItem, TraderItem)
        If chosen IsNot Nothing Then keepId = chosen.Id

        cmbTrader.Items.Clear()
        cmbTrader.Items.Add(New TraderItem With {.Id = 0, .Name = "All traders"})

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Using cmd As New SqlCommand("SELECT TraderID, FullName FROM Traders ORDER BY FullName", conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            cmbTrader.Items.Add(New TraderItem With {.Id = reader.GetInt32(0), .Name = reader.GetString(1)})
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load the traders: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        cmbTrader.SelectedIndex = 0
        For i As Integer = 0 To cmbTrader.Items.Count - 1
            If DirectCast(cmbTrader.Items(i), TraderItem).Id = keepId Then
                cmbTrader.SelectedIndex = i
                Exit For
            End If
        Next

        loadingTraders = False
    End Sub

    ' ================= LOAD AND ADD UP THE PAYMENTS =================
    Private Sub LoadData()
        If loadingTraders Then Return
        If cmbType.SelectedIndex < 0 OrElse cmbYear.SelectedIndex < 0 OrElse cmbMonth.SelectedIndex < 0 Then Return

        Dim traderItem As TraderItem = TryCast(cmbTrader.SelectedItem, TraderItem)
        Dim traderId As Integer = 0
        If traderItem IsNot Nothing Then traderId = traderItem.Id

        Dim typeFilter As String = ""
        If cmbType.SelectedIndex > 0 Then typeFilter = cmbType.SelectedItem.ToString()

        Dim yearFilter As Integer = 0
        If cmbYear.SelectedIndex > 0 Then yearFilter = Integer.Parse(cmbYear.SelectedItem.ToString())

        Dim monthFilter As Integer = cmbMonth.SelectedIndex   ' 0 = all, 1 to 12 = month number

        Dim table As New DataTable()
        table.Columns.Add("Receipt No", GetType(String))
        table.Columns.Add("Date", GetType(DateTime))
        table.Columns.Add("Trader", GetType(String))
        table.Columns.Add("Tax Type", GetType(String))
        table.Columns.Add("Period", GetType(String))
        table.Columns.Add("Income (GHS)", GetType(Decimal))
        table.Columns.Add("Rate (%)", GetType(Decimal))
        table.Columns.Add("Amount (GHS)", GetType(Decimal))

        Dim total As Decimal = 0D
        Dim payers As New HashSet(Of Integer)
        Dim byType As New Dictionary(Of String, Decimal)
        For Each t As String In taxTypes
            byType(t) = 0D
        Next

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT p.TraderID, p.Reference, p.PaymentDate, t.FullName, p.TaxType, p.PayMonth, p.PayYear, " &
                    "p.Income, p.Rate, p.Amount " &
                    "FROM Payments p INNER JOIN Traders t ON t.TraderID = p.TraderID " &
                    "WHERE (@type = '' OR p.TaxType = @type) AND (@y = 0 OR p.PayYear = @y) " &
                    "AND (@m = 0 OR p.PayMonth = @m) AND (@t = 0 OR p.TraderID = @t) " &
                    "ORDER BY p.PaymentDate DESC"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@type", typeFilter)
                    cmd.Parameters.AddWithValue("@y", yearFilter)
                    cmd.Parameters.AddWithValue("@m", monthFilter)
                    cmd.Parameters.AddWithValue("@t", traderId)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim amount As Decimal = reader.GetDecimal(9)
                            Dim taxType As String = reader.GetString(4)
                            total += amount
                            payers.Add(reader.GetInt32(0))
                            If byType.ContainsKey(taxType) Then byType(taxType) += amount

                            table.Rows.Add(reader.GetString(1),
                                           reader.GetDateTime(2),
                                           reader.GetString(3),
                                           taxType,
                                           monthNames(reader.GetInt32(5) - 1) & " " & reader.GetInt32(6),
                                           reader.GetDecimal(7),
                                           reader.GetDecimal(8),
                                           amount)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load the report: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        currentTable = table
        currentTotal = total

        grid.DataSource = table
        grid.Columns("Receipt No").FillWeight = 24
        grid.Columns("Date").FillWeight = 17
        grid.Columns("Trader").FillWeight = 20
        grid.Columns("Tax Type").FillWeight = 20
        grid.Columns("Period").FillWeight = 14
        grid.Columns("Income (GHS)").FillWeight = 14
        grid.Columns("Rate (%)").FillWeight = 8
        grid.Columns("Amount (GHS)").FillWeight = 14
        grid.Columns("Date").DefaultCellStyle.Format = "dd MMM yyyy HH:mm"
        grid.Columns("Income (GHS)").DefaultCellStyle.Format = "N2"
        grid.Columns("Rate (%)").DefaultCellStyle.Format = "0.##"
        grid.Columns("Amount (GHS)").DefaultCellStyle.Format = "N2"
        grid.Columns("Income (GHS)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        grid.Columns("Rate (%)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        grid.Columns("Amount (GHS)").DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

        Dim average As Decimal = 0D
        If table.Rows.Count > 0 Then average = Math.Round(total / table.Rows.Count, 2)

        currentBreakdown = ""
        For Each t As String In taxTypes
            If currentBreakdown <> "" Then currentBreakdown &= "   |   "
            currentBreakdown &= t & ": " & FormatMoney(byType(t))
        Next

        lblTotal.Text = "Total collected: " & FormatMoney(total)
        lblDetails.Text = "Payments: " & table.Rows.Count & "   |   Traders who paid: " & payers.Count &
                          "   |   Average payment: " & FormatMoney(average) & vbCrLf & currentBreakdown

        UpdateSelected(Nothing, EventArgs.Empty)
    End Sub

    ' ================= ADD UP SELECTED ROWS =================
    Private Sub UpdateSelected(sender As Object, e As EventArgs)
        If lblSelected Is Nothing Then Return

        Dim count As Integer = 0
        Dim sum As Decimal = 0D
        For Each row As DataGridViewRow In grid.SelectedRows
            Dim v As Object = row.Cells("Amount (GHS)").Value
            If v IsNot Nothing AndAlso Not IsDBNull(v) Then
                sum += Convert.ToDecimal(v)
                count += 1
            End If
        Next

        If count = 0 Then
            lblSelected.Text = "Tip: select rows (hold Ctrl or Shift) to add up only those payments."
            lblSelected.ForeColor = Color.Gray
        Else
            lblSelected.Text = "Selected: " & count & " payment(s) = " & FormatMoney(sum)
            lblSelected.ForeColor = Color.FromArgb(30, 70, 140)
        End If
    End Sub

    ' ================= PRINT =================
    Private Function Trunc(text As String, length As Integer) As String
        If text.Length > length Then Return text.Substring(0, length - 1) & "."
        Return text
    End Function

    Private Sub Print_Click(sender As Object, e As EventArgs)
        If currentTable Is Nothing OrElse currentTable.Rows.Count = 0 Then
            MessageBox.Show("There are no payments to print for the selected filters.", "Nothing to print",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim rule As String = New String("-"c, 86)
        printLines = New List(Of String)
        printLines.Add("TAX COLLECTION SYSTEM - PAYMENT REPORT")
        printLines.Add("Generated: " & DateTime.Now.ToString("dd MMM yyyy HH:mm"))
        printLines.Add("Filters: " & cmbType.Text & " | " & cmbYear.Text & " | " & cmbMonth.Text & " | " & cmbTrader.Text)
        printLines.Add("")
        printLines.Add("Date".PadRight(11) & " " & "Trader".PadRight(20) & " " & "Tax Type".PadRight(25) & " " &
                       "Period".PadRight(14) & " " & "Amount (GHS)".PadLeft(12))
        printLines.Add(rule)

        For Each r As DataRow In currentTable.Rows
            printLines.Add(Convert.ToDateTime(r("Date")).ToString("dd MMM yyyy").PadRight(11) & " " &
                           Trunc(Convert.ToString(r("Trader")), 20).PadRight(20) & " " &
                           Trunc(Convert.ToString(r("Tax Type")), 25).PadRight(25) & " " &
                           Trunc(Convert.ToString(r("Period")), 14).PadRight(14) & " " &
                           Convert.ToDecimal(r("Amount (GHS)")).ToString("N2").PadLeft(12))
        Next

        printLines.Add(rule)
        printLines.Add("Number of payments: " & currentTable.Rows.Count)
        printLines.Add("TOTAL COLLECTED: " & FormatMoney(currentTotal))
        printLines.Add("")
        For Each part As String In currentBreakdown.Split(New String() {"   |   "}, StringSplitOptions.None)
            printLines.Add(part)
        Next

        Try
            Using doc As New PrintDocument()
                AddHandler doc.BeginPrint, Sub(s, args) printIndex = 0
                AddHandler doc.PrintPage, AddressOf PrintPageHandler
                Using preview As New PrintPreviewDialog()
                    preview.Document = doc
                    preview.WindowState = FormWindowState.Maximized
                    preview.ShowDialog(Me.FindForm())
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not open the print preview: " & ex.Message, "Print error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub PrintPageHandler(sender As Object, e As PrintPageEventArgs)
        Using f As New Font("Consolas", 8)
            Dim lineHeight As Single = f.GetHeight(e.Graphics)
            Dim y As Single = e.MarginBounds.Top
            While printIndex < printLines.Count AndAlso y + lineHeight <= e.MarginBounds.Bottom
                e.Graphics.DrawString(printLines(printIndex), f, Brushes.Black, e.MarginBounds.Left, y)
                y += lineHeight
                printIndex += 1
            End While
            e.HasMorePages = (printIndex < printLines.Count)
        End Using
    End Sub

End Class