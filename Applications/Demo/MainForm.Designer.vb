<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.Label2 = New System.Windows.Forms.Label
		Me.Label3 = New System.Windows.Forms.Label
		Me.Label4 = New System.Windows.Forms.Label
		Me.tbRed = New System.Windows.Forms.TextBox
		Me.tbGreen = New System.Windows.Forms.TextBox
		Me.tbBlue = New System.Windows.Forms.TextBox
		Me.PictureBox1 = New System.Windows.Forms.PictureBox
		Me.cmdGenerate = New System.Windows.Forms.Button
		Me.cmdSave = New System.Windows.Forms.Button
		Me.lbStatus = New System.Windows.Forms.Label
		Me.ddPresets = New System.Windows.Forms.ComboBox
		Me.LinkLabel1 = New System.Windows.Forms.LinkLabel
		Me.Label1 = New System.Windows.Forms.Label
		Me.PictureBox2 = New System.Windows.Forms.PictureBox
		Me.lbStatus2 = New System.Windows.Forms.Label
		Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
		Me.Panel1 = New System.Windows.Forms.Panel
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.TableLayoutPanel1.SuspendLayout()
		Me.Panel1.SuspendLayout()
		Me.SuspendLayout()
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(4, 37)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(30, 13)
		Me.Label2.TabIndex = 1
		Me.Label2.Text = "Red:"
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(4, 62)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(39, 13)
		Me.Label3.TabIndex = 2
		Me.Label3.Text = "Green:"
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(4, 87)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(31, 13)
		Me.Label4.TabIndex = 3
		Me.Label4.Text = "Blue:"
		'
		'tbRed
		'
		Me.tbRed.Location = New System.Drawing.Point(52, 34)
		Me.tbRed.Name = "tbRed"
		Me.tbRed.Size = New System.Drawing.Size(256, 20)
		Me.tbRed.TabIndex = 2
		'
		'tbGreen
		'
		Me.tbGreen.Location = New System.Drawing.Point(52, 59)
		Me.tbGreen.Name = "tbGreen"
		Me.tbGreen.Size = New System.Drawing.Size(256, 20)
		Me.tbGreen.TabIndex = 3
		'
		'tbBlue
		'
		Me.tbBlue.Location = New System.Drawing.Point(52, 84)
		Me.tbBlue.Name = "tbBlue"
		Me.tbBlue.Size = New System.Drawing.Size(256, 20)
		Me.tbBlue.TabIndex = 4
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.Silver
		Me.PictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.PictureBox1.Location = New System.Drawing.Point(3, 171)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(275, 242)
		Me.PictureBox1.TabIndex = 7
		Me.PictureBox1.TabStop = False
		'
		'cmdGenerate
		'
		Me.cmdGenerate.Location = New System.Drawing.Point(314, 34)
		Me.cmdGenerate.Name = "cmdGenerate"
		Me.cmdGenerate.Size = New System.Drawing.Size(101, 32)
		Me.cmdGenerate.TabIndex = 5
		Me.cmdGenerate.Text = "Generate"
		Me.cmdGenerate.UseVisualStyleBackColor = True
		'
		'cmdSave
		'
		Me.cmdSave.Location = New System.Drawing.Point(314, 72)
		Me.cmdSave.Name = "cmdSave"
		Me.cmdSave.Size = New System.Drawing.Size(101, 32)
		Me.cmdSave.TabIndex = 6
		Me.cmdSave.Text = "Save Image"
		Me.cmdSave.UseVisualStyleBackColor = True
		'
		'lbStatus
		'
		Me.lbStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.lbStatus.Dock = System.Windows.Forms.DockStyle.Fill
		Me.lbStatus.Location = New System.Drawing.Point(3, 114)
		Me.lbStatus.Name = "lbStatus"
		Me.lbStatus.Size = New System.Drawing.Size(275, 54)
		Me.lbStatus.TabIndex = 10
		'
		'ddPresets
		'
		Me.ddPresets.FormattingEnabled = True
		Me.ddPresets.Location = New System.Drawing.Point(52, 7)
		Me.ddPresets.Name = "ddPresets"
		Me.ddPresets.Size = New System.Drawing.Size(140, 21)
		Me.ddPresets.TabIndex = 1
		Me.ddPresets.Text = "Select a preset"
		'
		'LinkLabel1
		'
		Me.LinkLabel1.AutoSize = True
		Me.LinkLabel1.LinkArea = New System.Windows.Forms.LinkArea(25, 13)
		Me.LinkLabel1.Location = New System.Drawing.Point(198, 10)
		Me.LinkLabel1.Name = "LinkLabel1"
		Me.LinkLabel1.Size = New System.Drawing.Size(217, 17)
		Me.LinkLabel1.TabIndex = 12
		Me.LinkLabel1.TabStop = True
		Me.LinkLabel1.Text = "Original Demo Concept by Pascal Ganaye"
		Me.LinkLabel1.UseCompatibleTextRendering = True
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(4, 10)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(42, 13)
		Me.Label1.TabIndex = 13
		Me.Label1.Text = "Presets"
		'
		'PictureBox2
		'
		Me.PictureBox2.BackColor = System.Drawing.Color.Silver
		Me.PictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.PictureBox2.Dock = System.Windows.Forms.DockStyle.Fill
		Me.PictureBox2.Location = New System.Drawing.Point(284, 171)
		Me.PictureBox2.Name = "PictureBox2"
		Me.PictureBox2.Size = New System.Drawing.Size(276, 242)
		Me.PictureBox2.TabIndex = 14
		Me.PictureBox2.TabStop = False
		'
		'lbStatus2
		'
		Me.lbStatus2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.lbStatus2.Dock = System.Windows.Forms.DockStyle.Fill
		Me.lbStatus2.Location = New System.Drawing.Point(284, 114)
		Me.lbStatus2.Name = "lbStatus2"
		Me.lbStatus2.Size = New System.Drawing.Size(276, 54)
		Me.lbStatus2.TabIndex = 15
		'
		'TableLayoutPanel1
		'
		Me.TableLayoutPanel1.ColumnCount = 2
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
		Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 0, 0)
		Me.TableLayoutPanel1.Controls.Add(Me.PictureBox2, 1, 2)
		Me.TableLayoutPanel1.Controls.Add(Me.PictureBox1, 0, 2)
		Me.TableLayoutPanel1.Controls.Add(Me.lbStatus2, 1, 1)
		Me.TableLayoutPanel1.Controls.Add(Me.lbStatus, 0, 1)
		Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.TableLayoutPanel1.Location = New System.Drawing.Point(3, 3)
		Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
		Me.TableLayoutPanel1.RowCount = 3
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 114.0!))
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54.0!))
		Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
		Me.TableLayoutPanel1.Size = New System.Drawing.Size(563, 416)
		Me.TableLayoutPanel1.TabIndex = 16
		'
		'Panel1
		'
		Me.TableLayoutPanel1.SetColumnSpan(Me.Panel1, 2)
		Me.Panel1.Controls.Add(Me.Label1)
		Me.Panel1.Controls.Add(Me.Label2)
		Me.Panel1.Controls.Add(Me.LinkLabel1)
		Me.Panel1.Controls.Add(Me.Label3)
		Me.Panel1.Controls.Add(Me.ddPresets)
		Me.Panel1.Controls.Add(Me.Label4)
		Me.Panel1.Controls.Add(Me.cmdSave)
		Me.Panel1.Controls.Add(Me.tbRed)
		Me.Panel1.Controls.Add(Me.cmdGenerate)
		Me.Panel1.Controls.Add(Me.tbGreen)
		Me.Panel1.Controls.Add(Me.tbBlue)
		Me.Panel1.Dock = System.Windows.Forms.DockStyle.Top
		Me.Panel1.Location = New System.Drawing.Point(0, 0)
		Me.Panel1.Margin = New System.Windows.Forms.Padding(0)
		Me.Panel1.Name = "Panel1"
		Me.Panel1.Size = New System.Drawing.Size(563, 113)
		Me.Panel1.TabIndex = 17
		'
		'MainForm
		'
		Me.AcceptButton = Me.cmdGenerate
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(569, 422)
		Me.Controls.Add(Me.TableLayoutPanel1)
		Me.MinimumSize = New System.Drawing.Size(435, 380)
		Me.Name = "MainForm"
		Me.Padding = New System.Windows.Forms.Padding(3)
		Me.Text = "FLEE and ILCalc Comparsion Demo"
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
		Me.TableLayoutPanel1.ResumeLayout(False)
		Me.Panel1.ResumeLayout(False)
		Me.Panel1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
	Friend WithEvents Label2 As System.Windows.Forms.Label
	Friend WithEvents Label3 As System.Windows.Forms.Label
	Friend WithEvents Label4 As System.Windows.Forms.Label
	Friend WithEvents tbRed As System.Windows.Forms.TextBox
	Friend WithEvents tbGreen As System.Windows.Forms.TextBox
	Friend WithEvents tbBlue As System.Windows.Forms.TextBox
	Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
	Friend WithEvents cmdGenerate As System.Windows.Forms.Button
	Friend WithEvents cmdSave As System.Windows.Forms.Button
	Friend WithEvents lbStatus As System.Windows.Forms.Label
	Friend WithEvents ddPresets As System.Windows.Forms.ComboBox
	Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
	Friend WithEvents Label1 As System.Windows.Forms.Label
	Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
	Friend WithEvents lbStatus2 As System.Windows.Forms.Label
	Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
	Friend WithEvents Panel1 As System.Windows.Forms.Panel

End Class
