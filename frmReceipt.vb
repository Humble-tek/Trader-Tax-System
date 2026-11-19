Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Windows.Forms

Public Class frmReceipt

    Private receiptText As String

    Public Sub New(reference As String, paidOn As DateTime, traderName As String, taxType As String,
                   period As String, income As Decimal, rate As Decimal, amount As Decimal, phone As String)
        InitializeComponent()

        Dim line As String = "----------------------------------------"
        receiptText =
            "          REPUBLIC OF GHANA" & vbCrLf &
            "        TAX COLLECTION SYSTEM" & vbCrLf &
            "      OFFICIAL PAYMENT RECEIPT" & vbCrLf &
            line & vbCrLf &
            "Receipt No : " & reference & vbCrLf &
            "Date       : " & paidOn.ToString("dd MMM yyyy HH:mm") & vbCrLf &
            "Trader     : " & traderName & vbCrLf &
            "Phone      : " & MaskPhone(phone) & vbCrLf &
            line & vbCrLf &
            "Tax Type   : " & taxType & vbCrLf &
            "Period     : " & period & vbCrLf &
            "Income     : " & FormatMoney(income) & vbCrLf &
            "Rate       : " & rate.ToString("0.##") & "%" & vbCrLf &
            line & vbCrLf &
            "AMOUNT PAID: " & FormatMoney(amount) & vbCrLf &
            "STATUS     : PAID" & vbCrLf &
            line & vbCrLf &
            "   Thank you for paying your tax."
    End Sub

    Private Function MaskPhone(p As String) As String
        If p.Length >= 10 Then Return p.Substring(0, 3) & "**" & p.Substring(7)
        Return p
    End Function

    Private Sub frmReceipt_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Payment Receipt"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.StartPosition = FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Size = New Size(520, 600)
        Me.BackColor = Color.White

        Dim card As New FlowLayoutPanel()
        card.Dock = DockStyle.Fill
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.Padding = New Padding(20)
        Me.Controls.Add(card)

        Dim paper As New Panel()
        paper.BorderStyle = BorderStyle.FixedSingle
        paper.BackColor = Color.FromArgb(252, 252, 245)
        paper.Padding = New Padding(15)
        paper.AutoSize = True

        Dim lbl As New Label()
        lbl.Text = receiptText
        lbl.Font = New Font("Consolas", 10)
        lbl.AutoSize = True
        paper.Controls.Add(lbl)
        card.Controls.Add(paper)

        Dim btnPrint As Button = NewButton("Print Receipt", Color.FromArgb(30, 70, 140))
        btnPrint.Margin = New Padding(3, 15, 3, 3)
        AddHandler btnPrint.Click, AddressOf Print_Click
        card.Controls.Add(btnPrint)

        Dim btnClose As Button = NewButton("Close", Color.FromArgb(110, 110, 110))
        AddHandler btnClose.Click, Sub(s, args) Me.Close()
        card.Controls.Add(btnClose)
    End Sub

    Private Sub Print_Click(sender As Object, e As EventArgs)
        Try
            Using doc As New PrintDocument()
                AddHandler doc.PrintPage, Sub(s, args) args.Graphics.DrawString(receiptText, New Font("Consolas", 12), Brushes.Black, 60, 60)
                Using preview As New PrintPreviewDialog()
                    preview.Document = doc
                    preview.WindowState = FormWindowState.Maximized
                    preview.ShowDialog(Me)
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not open the print preview: " & ex.Message, "Print error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class