Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucComplaints

    Private txtSubject As TextBox
    Private txtMessage As TextBox
    Private grid As DataGridView
    Private txtDetail As TextBox
    Private lblEmpty As Label

    ' ================= BUILD THE PAGE =================
    Private Sub ucComplaints_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = True
        Me.AutoScrollMinSize = New Size(620, 760)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 1
        layout.RowCount = 7
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 0 title
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 1 form
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 2 "My Complaints"
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 3 empty message
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))      ' 4 table
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))          ' 5 detail caption
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 150))     ' 6 detail box
        Me.Controls.Add(layout)

        ' ---------- Title ----------
        layout.Controls.Add(NewLabel("Complaints", 22, True), 0, 0)

        ' ---------- Complaint form ----------
        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True

        card.Controls.Add(NewLabel("Subject:"))
        txtSubject = NewTextBox()
        txtSubject.Width = 500
        txtSubject.MaxLength = 100
        card.Controls.Add(txtSubject)

        card.Controls.Add(NewLabel("Your complaint:"))
        txtMessage = NewTextBox()
        txtMessage.Width = 500
        txtMessage.Height = 90
        txtMessage.Multiline = True
        txtMessage.ScrollBars = ScrollBars.Vertical
        txtMessage.MaxLength = 1000
        card.Controls.Add(txtMessage)

        Dim btnSubmit As Button = NewButton("Submit Complaint")
        btnSubmit.Margin = New Padding(3, 10, 3, 3)
        AddHandler btnSubmit.Click, AddressOf Submit_Click
        card.Controls.Add(btnSubmit)

        layout.Controls.Add(card, 0, 1)

        ' ---------- My complaints ----------
        Dim headerRow As New FlowLayoutPanel()
        headerRow.FlowDirection = FlowDirection.LeftToRight
        headerRow.AutoSize = True
        headerRow.Controls.Add(NewLabel("My Complaints", 14, True))

        Dim btnRefresh As Button = NewButton("Refresh", Color.FromArgb(110, 110, 110))
        btnRefresh.Width = 110
        btnRefresh.Height = 32
        btnRefresh.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btnRefresh.Margin = New Padding(20, 6, 3, 3)
        AddHandler btnRefresh.Click, Sub(s, args) LoadData()
        headerRow.Controls.Add(btnRefresh)
        layout.Controls.Add(headerRow, 0, 2)

        lblEmpty = NewLabel("You have not lodged any complaints yet.", 11)
        lblEmpty.ForeColor = Color.Gray
        lblEmpty.Visible = False
        layout.Controls.Add(lblEmpty, 0, 3)

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
        layout.Controls.Add(grid, 0, 4)

        ' ---------- Detail box ----------
        layout.Controls.Add(NewLabel("Selected complaint and admin reply:", 11, True), 0, 5)

        txtDetail = New TextBox()
        txtDetail.Dock = DockStyle.Fill
        txtDetail.Multiline = True
        txtDetail.ReadOnly = True
        txtDetail.ScrollBars = ScrollBars.Vertical
        txtDetail.Font = New Font("Segoe UI", 10)
        txtDetail.BackColor = Color.White
        layout.Controls.Add(txtDetail, 0, 6)

        LoadData()
    End Sub

    ' ================= SUBMIT A COMPLAINT =================
    Private Sub Submit_Click(sender As Object, e As EventArgs)
        Dim subject As String = txtSubject.Text.Trim()
        Dim message As String = txtMessage.Text.Trim()

        If subject.Length < 3 Then
            MessageBox.Show("Enter a subject of at least 3 characters.", "Missing subject",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If message.Length < 10 Then
            MessageBox.Show("Describe your complaint in at least 10 characters.", "Missing complaint",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "INSERT INTO Complaints (TraderID, Subject, Message, DateLodged) " &
                    "VALUES (@t, @s, @m, @d)"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@t", CurrentTraderID)
                    cmd.Parameters.AddWithValue("@s", subject)
                    cmd.Parameters.AddWithValue("@m", message)
                    cmd.Parameters.AddWithValue("@d", DateTime.Now)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show("Your complaint has been sent to the admin.", "Complaint lodged",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtSubject.Clear()
            txtMessage.Clear()
            LoadData()

        Catch ex As Exception
            MessageBox.Show("Could not send your complaint: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ================= LOAD MY COMPLAINTS =================
    Private Sub LoadData()
        Dim table As New DataTable()
        table.Columns.Add("ID", GetType(Integer))
        table.Columns.Add("Date", GetType(String))
        table.Columns.Add("Subject", GetType(String))
        table.Columns.Add("Status", GetType(String))
        table.Columns.Add("Complaint", GetType(String))
        table.Columns.Add("Reply", GetType(String))

        Try
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "SELECT ComplaintID, DateLodged, Subject, Message, Reply, ReplyDate " &
                    "FROM Complaints WHERE TraderID = @t ORDER BY DateLodged DESC"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@t", CurrentTraderID)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim reply As String = ""
                            Dim status As String = "Pending"
                            If Not reader.IsDBNull(4) AndAlso reader.GetString(4).Trim() <> "" Then
                                status = "Replied"
                                reply = reader.GetString(4)
                                If Not reader.IsDBNull(5) Then
                                    reply = reply & vbCrLf & vbCrLf & "(Replied on " &
                                            reader.GetDateTime(5).ToString("dd MMM yyyy HH:mm") & ")"
                                End If
                            End If
                            table.Rows.Add(reader.GetInt32(0),
                                           reader.GetDateTime(1).ToString("dd MMM yyyy HH:mm"),
                                           reader.GetString(2),
                                           status,
                                           reader.GetString(3),
                                           reply)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not load your complaints: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        grid.DataSource = table
        grid.Columns("ID").Visible = False
        grid.Columns("Complaint").Visible = False
        grid.Columns("Reply").Visible = False
        grid.Columns("Date").FillWeight = 22
        grid.Columns("Subject").FillWeight = 55
        grid.Columns("Status").FillWeight = 23

        lblEmpty.Visible = (table.Rows.Count = 0)
        grid.Visible = (table.Rows.Count > 0)
        If table.Rows.Count = 0 Then txtDetail.Text = ""
    End Sub

    ' Green for replied, orange for pending
    Private Sub ColourStatus(sender As Object, e As DataGridViewBindingCompleteEventArgs)
        For Each row As DataGridViewRow In grid.Rows
            Dim cell As DataGridViewCell = row.Cells("Status")
            If cell.Value IsNot Nothing AndAlso cell.Value.ToString() = "Replied" Then
                cell.Style.ForeColor = PrimaryColor
            Else
                cell.Style.ForeColor = Color.DarkOrange
            End If
            cell.Style.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        Next
    End Sub

    ' ================= SHOW THE SELECTED COMPLAINT =================
    Private Sub ShowDetail(sender As Object, e As EventArgs)
        If grid.CurrentRow Is Nothing OrElse grid.CurrentRow.Cells("Complaint").Value Is Nothing Then
            txtDetail.Text = ""
            Return
        End If

        Dim complaint As String = grid.CurrentRow.Cells("Complaint").Value.ToString()
        Dim reply As String = grid.CurrentRow.Cells("Reply").Value.ToString()

        Dim text As String = "YOUR COMPLAINT:" & vbCrLf & complaint & vbCrLf & vbCrLf & "ADMIN REPLY:" & vbCrLf
        If reply = "" Then
            text &= "No reply yet. Please check again later."
        Else
            text &= reply
        End If
        txtDetail.Text = text
    End Sub

End Class