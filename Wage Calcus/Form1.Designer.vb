<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblHoursWorked = New Label()
        lblRatePerHour = New Label()
        lblTotalAmount = New Label()
        lblTotalAmountDisplay = New Label()
        txtHoursworked = New TextBox()
        txtRatePerHour = New TextBox()
        btnCalculate = New Button()
        btnClear = New Button()
        btnExit = New Button()
        SuspendLayout()
        ' 
        ' lblHoursWorked
        ' 
        lblHoursWorked.AutoSize = True
        lblHoursWorked.BackColor = Color.Transparent
        lblHoursWorked.Location = New Point(44, 40)
        lblHoursWorked.Name = "lblHoursWorked"
        lblHoursWorked.Size = New Size(86, 15)
        lblHoursWorked.TabIndex = 0
        lblHoursWorked.Text = "Hours Worked:"
        ' 
        ' lblRatePerHour
        ' 
        lblRatePerHour.AutoSize = True
        lblRatePerHour.BackColor = Color.Transparent
        lblRatePerHour.Location = New Point(44, 72)
        lblRatePerHour.Name = "lblRatePerHour"
        lblRatePerHour.Size = New Size(83, 15)
        lblRatePerHour.TabIndex = 1
        lblRatePerHour.Text = "Rate Per Hour:"
        ' 
        ' lblTotalAmount
        ' 
        lblTotalAmount.AutoSize = True
        lblTotalAmount.BackColor = Color.Transparent
        lblTotalAmount.Location = New Point(44, 106)
        lblTotalAmount.Name = "lblTotalAmount"
        lblTotalAmount.Size = New Size(83, 15)
        lblTotalAmount.TabIndex = 2
        lblTotalAmount.Text = "Total Amount:"
        ' 
        ' lblTotalAmountDisplay
        ' 
        lblTotalAmountDisplay.AutoSize = True
        lblTotalAmountDisplay.BackColor = Color.Transparent
        lblTotalAmountDisplay.Location = New Point(152, 106)
        lblTotalAmountDisplay.Name = "lblTotalAmountDisplay"
        lblTotalAmountDisplay.Size = New Size(70, 15)
        lblTotalAmountDisplay.TabIndex = 3
        lblTotalAmountDisplay.Text = "....................."
        ' 
        ' txtHoursworked
        ' 
        txtHoursworked.Location = New Point(136, 32)
        txtHoursworked.Name = "txtHoursworked"
        txtHoursworked.Size = New Size(100, 23)
        txtHoursworked.TabIndex = 4
        ' 
        ' txtRatePerHour
        ' 
        txtRatePerHour.Location = New Point(136, 72)
        txtRatePerHour.Name = "txtRatePerHour"
        txtRatePerHour.Size = New Size(100, 23)
        txtRatePerHour.TabIndex = 5
        ' 
        ' btnCalculate
        ' 
        btnCalculate.BackColor = Color.FromArgb(CByte(0), CByte(192), CByte(0))
        btnCalculate.Location = New Point(44, 147)
        btnCalculate.Name = "btnCalculate"
        btnCalculate.Size = New Size(91, 23)
        btnCalculate.TabIndex = 6
        btnCalculate.Text = "C&alculate"
        btnCalculate.UseVisualStyleBackColor = False
        ' 
        ' btnClear
        ' 
        btnClear.BackColor = Color.DodgerBlue
        btnClear.Location = New Point(152, 147)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(75, 23)
        btnClear.TabIndex = 7
        btnClear.Text = "&Clear"
        btnClear.UseVisualStyleBackColor = False
        ' 
        ' btnExit
        ' 
        btnExit.BackColor = Color.Crimson
        btnExit.Location = New Point(106, 190)
        btnExit.Name = "btnExit"
        btnExit.Size = New Size(75, 23)
        btnExit.TabIndex = 8
        btnExit.Text = "E&xit"
        btnExit.UseVisualStyleBackColor = False
        ' 
        ' Form1
        ' 
        AcceptButton = btnCalculate
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Silver
        BackgroundImage = My.Resources.Resources.humble
        CancelButton = btnExit
        ClientSize = New Size(290, 216)
        Controls.Add(btnExit)
        Controls.Add(btnClear)
        Controls.Add(btnCalculate)
        Controls.Add(txtRatePerHour)
        Controls.Add(txtHoursworked)
        Controls.Add(lblTotalAmountDisplay)
        Controls.Add(lblTotalAmount)
        Controls.Add(lblRatePerHour)
        Controls.Add(lblHoursWorked)
        Name = "Form1"
        Text = "Wage Calcus"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblHoursWorked As Label
    Friend WithEvents lblRatePerHour As Label
    Friend WithEvents lblTotalAmount As Label
    Friend WithEvents lblTotalAmountDisplay As Label
    Friend WithEvents txtHoursworked As TextBox
    Friend WithEvents txtRatePerHour As TextBox
    Friend WithEvents btnCalculate As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnExit As Button

End Class
