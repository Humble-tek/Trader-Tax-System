Public Class Form1
    Private Sub btnCalculate_Click_10320031(sender As Object, e As EventArgs) Handles btnCalculate.Click
        Try
            'Determine variables
            Dim intHoursWorked As Integer
            Dim sngRatePerHour As Single
            Dim dblTotalAmount As Double



            'Handling empty data fields errors
            If txtHoursworked.Text = "" Then
                MessageBox.Show("Enter data into Hours Worked field.", "Empty Hours Worked field",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtHoursworked.Focus()


            ElseIf txtRatePerHour.Text = String.Empty Then
                MessageBox.Show("Enter data into Rate Per Hours field.", "Empty Rate Per Hours field",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtRatePerHour.Focus()
            End If

            'Handling non numeric
            If Not IsNumeric(txtHoursworked.Text) OrElse Not IsNumeric(txtRatePerHour.Text) Then
                MessageBox.Show("Fields must be numeric.", "INVALID INPUT",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                txtHoursworked.Focus()
                Exit Sub
            End If

            'check for values less than or equal to zero

            If txtHoursworked.Text <= 0 Then
                MessageBox.Show("The number entered in Hours worked field is less than Zero.", "INPUT ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtHoursworked.Focus()
                txtHoursworked.Clear()


            ElseIf txtRatePerHour.Text <= 0 Then
                MessageBox.Show("The number entered in Rate Per Hour field must not be less than Zero.", "INPUT ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtRatePerHour.Focus()
                txtRatePerHour.Clear()
            End If


            'Assign values
            intHoursWorked = CInt(txtHoursworked.Text)
            sngRatePerHour = CSng(txtRatePerHour.Text)


            'calculating values
            dblTotalAmount = intHoursWorked * sngRatePerHour

            'Displaying values
            lblTotalAmountDisplay.Text = dblTotalAmount.ToString("C2")
            MessageBox.Show("Your Total Amount is " & lblTotalAmountDisplay.Text, "TOTAL AMOUNT")
        Catch ex As Exception
        End Try

    End Sub

    Private Sub btnclear_Click_10320031(sender As Object, e As EventArgs) Handles btnClear.Click
        txtHoursworked.Clear()
        txtRatePerHour.Clear()

    End Sub

    Private Sub btnExit_Click_10320031(sender As Object, e As EventArgs) Handles btnExit.Click
        Me.Close()


    End Sub
End Class
