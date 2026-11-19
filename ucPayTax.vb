Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Data.SqlClient

Public Class ucPayTax

    Private cmbTaxType As ComboBox
    Private cmbMonth As ComboBox
    Private cmbYear As ComboBox
    Private txtIncome As TextBox
    Private lblRate As Label
    Private lblAmount As Label

    ' Tax rates in percent. Change these numbers to match your official rates.
    Private ReadOnly rates As New Dictionary(Of String, Decimal) From {
        {"Income Tax", 15D},
        {"Value Added Tax", 15D},
        {"Business Operating Permit", 5D}
    }

    Private ReadOnly monthNames As String() = {"January", "February", "March", "April", "May", "June",
                                               "July", "August", "September", "October", "November", "December"}

    ' ================= BUILD THE PAGE =================
    Private Sub ucPayTax_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = True

        Dim card As New FlowLayoutPanel()
        card.FlowDirection = FlowDirection.TopDown
        card.WrapContents = False
        card.AutoSize = True

        card.Controls.Add(NewLabel("Pay Tax", 22, True))
        card.Controls.Add(NewLabel("Payment is approved on your registered phone number.", 11))

        card.Controls.Add(NewLabel("Tax Type:"))
        cmbTaxType = MakeCombo()
        For Each name As String In rates.Keys
            cmbTaxType.Items.Add(name)
        Next
        AddHandler cmbTaxType.SelectedIndexChanged, AddressOf TaxType_Changed
        card.Controls.Add(cmbTaxType)

        card.Controls.Add(NewLabel("Month:"))
        cmbMonth = MakeCombo()
        For Each m As String In monthNames
            cmbMonth.Items.Add(m)
        Next
        cmbMonth.SelectedIndex = DateTime.Now.Month - 1
        card.Controls.Add(cmbMonth)

        card.Controls.Add(NewLabel("Year:"))
        cmbYear = MakeCombo()
        cmbYear.Items.Add((DateTime.Now.Year - 1).ToString())
        cmbYear.Items.Add(DateTime.Now.Year.ToString())
        cmbYear.SelectedIndex = 1
        card.Controls.Add(cmbYear)

        card.Controls.Add(NewLabel("Income or Sales Amount (GHS):"))
        txtIncome = NewTextBox()
        card.Controls.Add(txtIncome)

        lblRate = NewLabel("", 12, True)
        card.Controls.Add(lblRate)

        Dim btnCompute As Button = NewButton("Compute Tax", Color.FromArgb(30, 70, 140))
        btnCompute.Margin = New Padding(3, 15, 3, 3)
        AddHandler btnCompute.Click, AddressOf Compute_Click
        card.Controls.Add(btnCompute)

        lblAmount = NewLabel("Tax Amount: GHS 0.00", 16, True)
        card.Controls.Add(lblAmount)

        card.Controls.Add(NewLabel("Payment phone number: " & CurrentPhone & " (your registered number)", 11))

        Dim btnPay As Button = NewButton("Pay Now")
        btnPay.Margin = New Padding(3, 15, 3, 3)
        AddHandler btnPay.Click, AddressOf Pay_Click
        card.Controls.Add(btnPay)

        Me.Controls.Add(card)
        cmbTaxType.SelectedIndex = 0
    End Sub

    Private Function MakeCombo() As ComboBox
        Dim c As New ComboBox()
        c.DropDownStyle = ComboBoxStyle.DropDownList
        c.Font = New Font("Segoe UI", 11)
        c.Width = 280
        Return c
    End Function

    Private Sub TaxType_Changed(sender As Object, e As EventArgs)
        Dim rate As Decimal = rates(cmbTaxType.SelectedItem.ToString())
        lblRate.Text = "Rate: " & rate.ToString("0.##") & "%"
        lblAmount.Text = "Tax Amount: GHS 0.00"
    End Sub

    ' ================= CALCULATION =================
    Private Function TryGetAmount(ByRef income As Decimal, ByRef rate As Decimal, ByRef amount As Decimal) As Boolean
        If Not Decimal.TryParse(txtIncome.Text.Trim(), income) OrElse income <= 0 OrElse income > 999999999999D Then
            MessageBox.Show("Enter a valid income amount greater than 0.", "Invalid amount",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        rate = rates(cmbTaxType.SelectedItem.ToString())
        amount = Math.Round(income * rate / 100D, 2)
        If amount <= 0 Then
            MessageBox.Show("The tax amount is too small to pay. Enter a larger income amount.", "Invalid amount",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

    Private Sub Compute_Click(sender As Object, e As EventArgs)
        Dim income, rate, amount As Decimal
        If TryGetAmount(income, rate, amount) Then
            lblAmount.Text = "Tax Amount: " & FormatMoney(amount)
        End If
    End Sub

    ' ================= PAYMENT =================
    Private Sub Pay_Click(sender As Object, e As EventArgs)
        Dim income, rate, amount As Decimal
        If Not TryGetAmount(income, rate, amount) Then Return
        lblAmount.Text = "Tax Amount: " & FormatMoney(amount)

        Dim taxType As String = cmbTaxType.SelectedItem.ToString()
        Dim monthNo As Integer = cmbMonth.SelectedIndex + 1
        Dim yearNo As Integer = Integer.Parse(cmbYear.SelectedItem.ToString())
        Dim period As String = monthNames(monthNo - 1) & " " & yearNo

        Try
            ' 1. Stop double payment for the same tax and period
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim checkSql As String =
                    "SELECT COUNT(*) FROM Payments " &
                    "WHERE TraderID = @t AND TaxType = @type AND PayMonth = @m AND PayYear = @y"
                Using check As New SqlCommand(checkSql, conn)
                    check.Parameters.AddWithValue("@t", CurrentTraderID)
                    check.Parameters.AddWithValue("@type", taxType)
                    check.Parameters.AddWithValue("@m", monthNo)
                    check.Parameters.AddWithValue("@y", yearNo)
                    If Convert.ToInt32(check.ExecuteScalar()) > 0 Then
                        MessageBox.Show(taxType & " for " & period & " has already been paid.",
                                        "Already paid", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Return
                    End If
                End Using
            End Using

            ' 2. Ask for confirmation
            If MessageBox.Show("Pay " & FormatMoney(amount) & " for " & taxType & " (" & period & ")?" & vbCrLf &
                               "A payment request will be sent to " & CurrentPhone & ".",
                               "Confirm payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            ' 3. Phone approval prompt (simulated)
            Using prompt As New frmMoMoPrompt(CurrentPhone, amount)
                If prompt.ShowDialog(Me.FindForm()) <> DialogResult.OK Then
                    MessageBox.Show("The payment was not approved. No money was taken.", "Payment cancelled",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
            End Using

            ' 4. Save the payment
            Dim reference As String = GenerateReference()
            Dim paidOn As DateTime = DateTime.Now
            Using conn As SqlConnection = GetConnection()
                conn.Open()
                Dim sql As String =
                    "INSERT INTO Payments (TraderID, TaxType, PayMonth, PayYear, Income, Rate, Amount, Phone_Number, Reference, PaymentDate) " &
                    "VALUES (@t, @type, @m, @y, @income, @rate, @amount, @phone, @ref, @date)"
                Using cmd As New SqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@t", CurrentTraderID)
                    cmd.Parameters.AddWithValue("@type", taxType)
                    cmd.Parameters.AddWithValue("@m", monthNo)
                    cmd.Parameters.AddWithValue("@y", yearNo)
                    cmd.Parameters.AddWithValue("@income", income)
                    cmd.Parameters.AddWithValue("@rate", rate)
                    cmd.Parameters.AddWithValue("@amount", amount)
                    cmd.Parameters.AddWithValue("@phone", CurrentPhone)
                    cmd.Parameters.AddWithValue("@ref", reference)
                    cmd.Parameters.AddWithValue("@date", paidOn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            ' 5. Show the receipt
            txtIncome.Clear()
            lblAmount.Text = "Tax Amount: GHS 0.00"
            Using receipt As New frmReceipt(reference, paidOn, CurrentFullName, taxType, period,
                                            income, rate, amount, CurrentPhone)
                receipt.ShowDialog(Me.FindForm())
            End Using

        Catch ex As Exception
            MessageBox.Show("Payment failed: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class