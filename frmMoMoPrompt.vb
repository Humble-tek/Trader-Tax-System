Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class frmMoMoPrompt

    Private payPhone As String
    Private payAmount As Decimal
    Private lblStatus As Label
    Private txtPin As TextBox
    Private btnApprove As Button
    Private tmr As System.Windows.Forms.Timer

    Public Sub New(phone As String, amount As Decimal)
        InitializeComponent()
        payPhone = phone
        payAmount = amount
    End Sub

    Private Function MaskPhone(p As String) As String
        If p.Length >= 10 Then Return p.Substring(0, 3) & "**" & p.Substring(7)
        Return p
    End Function

    Private Sub frmMoMoPrompt_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Mobile Money Payment Request"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ControlBox = False
        Me.ShowInTaskbar = False
        Me.Size = New Size(440, 520)
        Me.BackColor = Color.White

        Dim card As New FlowLayoutPanel()
        card.Dock = DockStyle.Fill
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.Padding = New Padding(25)
        Me.Controls.Add(card)

        card.Controls.Add(NewLabel("Payment Request", 18, True))
        card.Controls.Add(NewLabel("Amount: " & FormatMoney(payAmount), 14, True))
        card.Controls.Add(NewLabel("Phone: " & MaskPhone(payPhone), 12))

        lblStatus = NewLabel("Sending the payment request to your phone...", 11)
        lblStatus.MaximumSize = New Size(360, 0)
        card.Controls.Add(lblStatus)

        card.Controls.Add(NewLabel("Enter your 4-digit Mobile Money PIN:"))
        txtPin = NewTextBox(True)
        txtPin.MaxLength = 4
        txtPin.Enabled = False
        card.Controls.Add(txtPin)

        btnApprove = NewButton("Approve Payment")
        btnApprove.Margin = New Padding(3, 15, 3, 3)
        btnApprove.Enabled = False
        AddHandler btnApprove.Click, AddressOf Approve_Click
        card.Controls.Add(btnApprove)

        Dim btnDecline As Button = NewButton("Decline", DangerColor)
        AddHandler btnDecline.Click, AddressOf Decline_Click
        card.Controls.Add(btnDecline)
        Me.CancelButton = btnDecline

        Dim lblNote As Label = NewLabel("Simulation: any 4-digit PIN approves. A real mobile money service would send this prompt to the phone.", 9)
        lblNote.ForeColor = Color.Gray
        lblNote.MaximumSize = New Size(360, 0)
        card.Controls.Add(lblNote)

        ' Wait 3 seconds, as if the request is travelling to the phone
        tmr = New System.Windows.Forms.Timer()
        tmr.Interval = 3000
        AddHandler tmr.Tick, AddressOf Tmr_Tick
        tmr.Start()
    End Sub

    Private Sub Tmr_Tick(sender As Object, e As EventArgs)
        tmr.Stop()
        lblStatus.Text = "A payment request of " & FormatMoney(payAmount) & " was sent to " &
                         MaskPhone(payPhone) & ". Enter your PIN to approve."
        txtPin.Enabled = True
        btnApprove.Enabled = True
        txtPin.Focus()
    End Sub

    Private Sub Approve_Click(sender As Object, e As EventArgs)
        If Not Regex.IsMatch(txtPin.Text, "^\d{4}$") Then
            MessageBox.Show("The PIN must be exactly 4 digits.", "Invalid PIN",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub Decline_Click(sender As Object, e As EventArgs)
        Me.DialogResult = DialogResult.Cancel
    End Sub

    Private Sub frmMoMoPrompt_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If tmr IsNot Nothing Then
            tmr.Stop()
            tmr.Dispose()
        End If
    End Sub

End Class