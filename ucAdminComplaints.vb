Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucAdminComplaints

    Private cmbFilter As ComboBox
    Private grid As DataGridView
    Private txtMessage As TextBox
    Private txtReply As TextBox
    Private lblSummary As Label
    Private table As DataTable
    Private view As DataView

    ' ================= BUILD THE PAGE =================
    Private Sub ucAdminComplaints_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = True
        Me.AutoScrollMinSize = New Size(700, 820)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 9
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 0 title
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 1 filter
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 2 summary
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))      ' 3 table
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 4 details caption
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 140))     ' 5 details box
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 6 reply caption
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 90))      ' 7 reply box
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 8 button
        Me.Controls.Add(layout)

        layout.Controls.Add(NewLabel("Trader Complaints", 22, True), 0, 0)

        ' ---------- Filter ----------
        Dim filterRow As New FlowLayoutPanel()
        filterRow.FlowDirection = FlowDirection.LeftToRight
        filterRow.AutoSize = True
        filterRow.Controls.Add(NewLabel("Show:"))

        cmbFilter = New ComboBox()
        cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFilter.Font = New Font("Segoe UI", 11)
        cmbFilter.Width = 160
        cmbFilter.Margin = New Padding(3, 5, 15, 3)
        cmbFilter.Items.Add("All complaints")
        cmbFilter.Items.Add("Pending only")
        cmbFilter.Items.Add("Replied only")
        cmbFilter.SelectedIndex = 0
        filterRow.Controls.Add(cmbFilter)

        Dim btnRefresh As Button = NewButton("Refresh", Color.FromArgb(110, 110, 110))
        btnRefresh.Width = 110
        btnRefresh.Height = 32
        btnRefresh.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnRefresh.Margin = New Padding(3, 5, 3, 3)
        AddHandler btnRefresh.Click, Sub(s, args) LoadData(0)
        filterRow.Controls.Add(btnRefresh)
        layout.Controls.Add(filterRow, 0, 1)

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
        grid.ColumnHeadersHeight = 36
        grid.RowTemplate.Height = 30
        AddHandler grid.DataBindingComplete, AddressOf ColourStatus
        AddHandler grid.SelectionChanged, AddressOf ShowDetail
        layout.Controls.Add(grid, 0, 3)

        ' ---------- Details ----------
        layout.Controls.Add(NewLabel("Selected complaint:", 11, True), 0, 4)

        txtMessage = New TextBox()
        txtMessage.Dock = DockStyle.Fill
        txtMessage.Multiline = True
        txtMessage.ReadOnly = True
        txtMessage.ScrollBars = ScrollBars.Vertical
        txtMessage.Font = New Font("Segoe UI", 10)
        txtMessage.BackColor = Color.White
        layout.Controls.Add(txtMessage, 0, 5)

        ' ---------- Reply ----------
        layout.Controls.Add(NewLabel("Your reply to the trader:", 11, True), 0, 6)

        txtReply = New TextBox()
        txtReply.Dock = DockStyle.Fill
        txtReply.Multiline = True
        txtReply.ScrollBars = ScrollBars.Vertical
        txtReply.Font = New Font("Segoe UI", 10)
        txtReply.MaxLength = 1000
        layout.Controls.Add(txtReply, 0, 7)

        Dim btnSend As Button = NewButton("Send Reply")
        btnSend.Margin = New Padding(3, 10, 3, 3)
        AddHandler btnSend.Click, AddressOf SendReply_Click
        layout.Controls.Add(btnSend, 0, 8)

        AddHandler cmbFilter.SelectedIndexChanged, AddressOf Filter_Changed

        LoadData(0)
    End Sub

    ' ================= LOAD ALL COMPLAINTS =================
    ' keepId: the complaint to select again after loading (0 = none)
    Private Sub LoadData(keepId As Integer)
        table = New DataTable()
        table.Columns.Add("ID", GetType(Integer))
        table.Columns.Add("Date", GetType(DateTime))
        table.Columns.Add("Trader", GetType(String))
        table.Columns.Add("Subject", GetType(String))
        table.Columns.Add("Status", GetType(String))
        table.Columns.Add("Phone", GetType(String))
        table.Columns.Add("Complaint", GetType(String))
        table.Columns.Add("Reply", GetType(String))
        table.Columns.Add("ReplyDate", GetType(String))

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT c.ComplaintID, c.DateLodged, t.FullName, c.Subject, c.Message, " &
                    "c.Reply, c.ReplyDate, t.Phone_Number " &
                    "FROM Complaints c INNER JOIN Traders t ON t.TraderID = c.TraderID " &
                    "ORDER BY CASE WHEN c.Reply IS NULL OR LTRIM(RTRIM(c.Reply)) = '' THEN 0 ELSE 1 END, " &
                    "c.DateLodged DESC"
                Using cmd As New SqlCommand(sql, conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim reply As String = ""
                            Dim replyDate As String = ""
                            Dim status As String = "Pending"
                            If Not reader.IsDBNull(5) AndAlso reader.GetString(5).Trim() <> "" Then
                                status = "Replied"
                                reply = reader.GetString(5)
                                If Not reader.IsDBNull(6) Then
                                    replyDate = reader.GetDateTime(6).ToString("dd MMM yyyy HH:mm")
                                End If
                            End If
                            table.Rows.Add(reader.GetInt32(0),
                                           reader.GetDateTime(1),
                                           reader.GetString(2),
                                           reader.GetString(3),
                                           status,
                                           reader.GetString(7).Trim(),
                                           reader.GetString(4),
                                           reply,
                                           replyDate)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load the complaints: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        view = New DataView(table)
        grid.DataSource = view

        grid.Columns("ID").Visible = False
        grid.Columns("Phone").Visible = False
        grid.Columns("Complaint").Visible = False
        grid.Columns("Reply").Visible = False
        grid.Columns("ReplyDate").Visible = False
        grid.Columns("Date").DefaultCellStyle.Format = "dd MMM yyyy HH:mm"
        grid.Columns("Date").FillWeight = 22
        grid.Columns("Trader").FillWeight = 24
        grid.Columns("Subject").FillWeight = 38
        grid.Columns("Status").FillWeight = 16

        ApplyFilter()

        ' Select the complaint the admin was working on
        If keepId > 0 Then
            For i As Integer = 0 To grid.Rows.Count - 1
                If Convert.ToInt32(grid.Rows(i).Cells("ID").Value) = keepId Then
                    grid.CurrentCell = grid.Rows(i).Cells("Subject")
                    grid.Rows(i).Selected = True
                    Exit For
                End If
            Next
        End If
        ShowDetail(Nothing, EventArgs.Empty)
    End Sub

    ' ================= FILTER =================
    Private Sub Filter_Changed(sender As Object, e As EventArgs)
        ApplyFilter()
        ShowDetail(Nothing, EventArgs.Empty)
    End Sub

    Private Sub ApplyFilter()
        If view Is Nothing Then Return

        Select Case cmbFilter.SelectedIndex
            Case 1
                view.RowFilter = "Status = 'Pending'"
            Case 2
                view.RowFilter = "Status = 'Replied'"
            Case Else
                view.RowFilter = ""
        End Select

        Dim pending As Integer = table.Select("Status = 'Pending'").Length
        Dim replied As Integer = table.Select("Status = 'Replied'").Length
        lblSummary.Text = "Total: " & table.Rows.Count & "   |   Pending: " & pending & "   |   Replied: " & replied
    End Sub

    ' Green for replied, orange for pending
    Private Sub ColourStatus(sender As Object, e As DataGridViewBindingCompleteEventArgs)
        For Each row As DataGridViewRow In grid.Rows
            Dim cell As DataGridViewCell = row.Cells("Status")
            If Convert.ToString(cell.Value) = "Replied" Then
                cell.Style.ForeColor = PrimaryColor
            Else
                cell.Style.ForeColor = Color.DarkOrange
            End If
            cell.Style.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        Next
    End Sub

    ' ================= SHOW THE SELECTED COMPLAINT =================
    Private Sub ShowDetail(sender As Object, e As EventArgs)
        If txtMessage Is Nothing Then Return

        If grid.CurrentRow Is Nothing OrElse grid.CurrentRow.Cells("Complaint").Value Is Nothing Then
            txtMessage.Text = ""
            Return
        End If

        Dim row As DataGridViewRow = grid.CurrentRow
        Dim text As String =
            "FROM: " & Convert.ToString(row.Cells("Trader").Value) &
            "  (Phone: " & Convert.ToString(row.Cells("Phone").Value) & ")" & vbCrLf &
            "DATE: " & Convert.ToDateTime(row.Cells("Date").Value).ToString("dd MMM yyyy HH:mm") & vbCrLf &
            "SUBJECT: " & Convert.ToString(row.Cells("Subject").Value) & vbCrLf & vbCrLf &
            Convert.ToString(row.Cells("Complaint").Value)

        Dim reply As String = Convert.ToString(row.Cells("Reply").Value)
        If reply <> "" Then
            text &= vbCrLf & vbCrLf & "---- CURRENT REPLY (sent " &
                    Convert.ToString(row.Cells("ReplyDate").Value) & ") ----" & vbCrLf & reply
        End If

        txtMessage.Text = text
    End Sub

    ' ================= SEND A REPLY =================
    Private Sub SendReply_Click(sender As Object, e As EventArgs)
        If grid.CurrentRow Is Nothing OrElse grid.CurrentRow.Cells("ID").Value Is Nothing Then
            MessageBox.Show("Select a complaint in the table first.", "No complaint selected",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim reply As String = txtReply.Text.Trim()
        If reply.Length < 3 Then
            MessageBox.Show("Type a reply of at least 3 characters.", "Reply is empty",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim id As Integer = Convert.ToInt32(grid.CurrentRow.Cells("ID").Value)
        Dim alreadyReplied As Boolean = (Convert.ToString(grid.CurrentRow.Cells("Status").Value) = "Replied")

        If alreadyReplied Then
            If MessageBox.Show("This complaint already has a reply. Replace it with your new reply?",
                               "Replace reply", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String = "UPDATE Complaints SET Reply = @r, ReplyDate = @d WHERE ComplaintID = @id"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@r", reply)
                    cmd.Parameters.AddWithValue("@d", DateTime.Now)
                    cmd.Parameters.AddWithValue("@id", id)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Your reply has been sent. The trader can now see it.", "Reply sent",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtReply.Clear()
            LoadData(id)

        Catch ex As Exception
            MessageBox.Show("Could not send the reply: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class