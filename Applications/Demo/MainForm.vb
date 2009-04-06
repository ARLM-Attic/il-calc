' Demo application for compiled runtime expressions.
' Generates an image by evaluating expressions for the r,g,b components of each pixel.
' Author: Eugene Ciloci
' Original demo concept by Pascal Ganaye (http://www.codeproject.com/dotnet/eval3.asp)

' Modified by Alexander Shvedov
' for comparsion demo

Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Globalization
Imports ILCalc
Imports System.Threading

Public Class MainForm

	' Each expression will be attached to this class and will have access to the X and Y fields
	Private Class ExpressionOwner

		Public X As Double
		Public Y As Double

	End Class

	Private Delegate Sub Action()

	Private MyImageWidth As Integer
	Private MyImageHeight As Integer
	Private MyImageHalfWidth As Integer
	Private MyImageHalfHeight As Integer
	Private MyImageBitmapSize As Integer

	Private Const ORIGINAL_CONCEPT_URL As String = "http://www.codeproject.com/dotnet/eval3.asp"

	' The red, green, and blue expressions
	Private MyRedExpression, MyGreenExpression, MyBlueExpression As IGenericExpression(Of Double)
	' Our raw image data
	Private MyImageData, MyImageData2 As Byte()
	' Use one expression owner instance for all expressions
	Private MyExpressionOwner As ExpressionOwner
	' Use same options for all expressions
	Private MyContext As ExpressionContext

	' ILCalc expressions context
	Private MyILCalcContext As CalcContext
	' The tabulators for red, green and blue expressions
	Private MyRedTabulator, MyGreenTabulator, MyBlueTabulator As Tabulator
	' The tabulators range
	Private MyRangeX As TabRange = New TabRange(-Math.PI, Math.PI, 2 * Math.PI / 240)
	Private MyRangeY As TabRange = New TabRange(-Math.PI, Math.PI, 2 * Math.PI / 240)

	Private Shared MyRandom As Random = New Random()

	Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)

		MyBase.OnLoad(e)
		MyExpressionOwner = New ExpressionOwner

		' Create the context with our owner
		MyContext = New ExpressionContext(MyExpressionOwner)
		' Import math so we have access to its functions in expressions
		MyContext.Imports.AddType(GetType(Math))
		MyContext.Imports.AddMethod("Rand", GetType(MainForm), "")

		InitSizes()

		Me.PictureBox1.Image = CreateInitialImage()
		Me.PictureBox2.Image = CreateInitialImage()

		MyImageData = New Byte(MyImageBitmapSize) {}
		MyImageData2 = New Byte(MyImageBitmapSize) {}

		Me.cmdGenerate.Enabled = False
		Me.PopulatePresets()
		Me.HookTextboxes()
		AddHandler ddPresets.SelectedIndexChanged, AddressOf ddPresets_FirstSelectedIndexChanged

		' Innitialize ILCalc
		MyILCalcContext = New CalcContext("x", "y")
		MyILCalcContext.Constants.ImportBuiltin()
		MyILCalcContext.Functions.ImportBuiltin()
		MyILCalcContext.Functions.Add(GetType(MainForm), "Rand")
		MyILCalcContext.Optimization = OptimizeModes.ConstantFolding Or OptimizeModes.PowOptimize
		MyILCalcContext.Culture = CultureInfo.CurrentCulture

	End Sub

	Public Shared Function Rand() As Double

		Return MyRandom.NextDouble()

	End Function

	Private Sub InitSizes()

		Dim Width As Integer = Me.PictureBox1.Width
		Dim Height As Integer = Me.PictureBox1.Height

		Width -= Width Mod 4
		Height -= Height Mod 4

		MyImageWidth = Width
		MyImageHeight = Height
		MyImageHalfWidth = Width \ 2
		MyImageHalfHeight = Height \ 2
		MyImageBitmapSize = Width * Height * 3

		MyRangeX.Step = 2 * Math.PI / (MyImageWidth + 1)
		MyRangeY.Step = 2 * Math.PI / (MyImageHeight + 1)

	End Sub

	' Create the initial image
	Private Function CreateInitialImage() As Image

		Dim img As Image = New Bitmap(MyImageWidth, MyImageHeight, Imaging.PixelFormat.Format24bppRgb)
		Dim g As Graphics = Graphics.FromImage(img)

		Dim imgRect As New Rectangle(0, 0, MyImageWidth, MyImageHeight)
		g.FillRectangle(Brushes.Silver, imgRect)
		g.DrawString("Select a preset or enter expressions for the red, green, and blue components.", _
		 New Font("Tahoma", 10, FontStyle.Bold), Brushes.Yellow, imgRect)
		g.Dispose()
		Return img

	End Function

	' Populate all the preset formulas
	Private Sub PopulatePresets()

		Dim info As PresetInfo
		Dim items As ComboBox.ObjectCollection = Me.ddPresets.Items

		info = New PresetInfo("Blinds", "(round(4*x-y*2) % 2) - x", "(abs(x+2*y) % 0{0}75)*10+y/5", "round(sin(sqrt(x*x+y*y))*3/5)+x/3")
		items.Add(info)
		info = New PresetInfo("Bullseye", "1-round(x/y*0{0}5)", "1-round(y/x*0{0}4)", "round(sin(sqrt(x*x+y*y)*10))")
		items.Add(info)
		info = New PresetInfo("Wave", "cos(x/2)/2", "cos(y/2)/3", "round(sin(sqrt(x*x*x+y*y)*10))")
		items.Add(info)
		info = New PresetInfo("Swirls", "x*15", "cos(x*y*4900)", "y*15")
		items.Add(info)
		info = New PresetInfo("Semi-Random", "cos(x) * rand()", "y^2", "x^2")
		items.Add(info)
		info = New PresetInfo("Mod", "(x ^2) % y", "y % x", "x % y")
		items.Add(info)

	End Sub

	Private Sub HookTextboxes()

		AddHandler tbRed.TextChanged, AddressOf ExpressionTextbox_TextChanged
		AddHandler tbGreen.TextChanged, AddressOf ExpressionTextbox_TextChanged
		AddHandler tbBlue.TextChanged, AddressOf ExpressionTextbox_TextChanged

	End Sub

	Private Sub UnHookTextboxes()
		RemoveHandler tbRed.TextChanged, AddressOf ExpressionTextbox_TextChanged
		RemoveHandler tbGreen.TextChanged, AddressOf ExpressionTextbox_TextChanged
		RemoveHandler tbBlue.TextChanged, AddressOf ExpressionTextbox_TextChanged
	End Sub

	' Function that actually generates the image
	Private Sub GenerateImage()

		Dim multX As Double = 2 * Math.PI / (MyImageWidth + 1)
		Dim multY As Double = 2 * Math.PI / (MyImageHeight + 1)
		Dim index As Integer

		For Yi As Integer = 0 To MyImageHeight - 1
			' Update the y coordinate
			MyExpressionOwner.Y = (Yi - MyImageHalfHeight) * multY

			For Xi As Integer = 0 To MyImageWidth - 1
				' Update the x coordinate
				MyExpressionOwner.X = (Xi - MyImageHalfWidth) * multX

				' Evaluate the expressions
				Me.SetColorComponent(MyImageData, MyBlueExpression.Evaluate(), index)
				Me.SetColorComponent(MyImageData, MyGreenExpression.Evaluate(), index + 1)
				Me.SetColorComponent(MyImageData, MyRedExpression.Evaluate(), index + 2)
				index += 3

			Next
		Next

	End Sub

	' Function that actually generates the image2
	Private Sub GenerateImage2()

		Dim index As Integer

		Dim arrBlue As Double()() = MyBlueTabulator.Tabulate(MyRangeX, MyRangeY)
		Dim arrGreen As Double()() = MyGreenTabulator.Tabulate(MyRangeX, MyRangeY)
		Dim arrRed As Double()() = MyRedTabulator.Tabulate(MyRangeX, MyRangeY)

		For Yi As Integer = 0 To MyImageHeight - 1

			For Xi As Integer = 0 To MyImageWidth - 1

				Me.SetColorComponent(MyImageData2, arrBlue(Xi)(Yi), index)
				Me.SetColorComponent(MyImageData2, arrGreen(Xi)(Yi), index + 1)
				Me.SetColorComponent(MyImageData2, arrRed(Xi)(Yi), index + 2)
				index += 3

			Next
		Next

	End Sub

	Private Sub SetColorComponent(ByVal imgData As Byte(), ByVal value As Double, ByVal index As Integer)

		If value < 0 Then
			value = 0
		ElseIf value > 1 Then
			value = 1
		ElseIf Double.IsNaN(value) = True Then
			value = 0
		End If

		imgData(index) = CByte(value * 255)

	End Sub

	Private Sub Generate(ByVal pBox As PictureBox, ByVal imgData As Byte(), ByVal generator As Action, ByVal lb As Label)

		Dim sw As New Stopwatch()

		sw.Start()
		' Time the image generation
		generator()
		sw.Stop()

		' Fast transfer of all the raw values to the image
		Dim rect As New Rectangle(0, 0, MyImageWidth, MyImageHeight)
		Dim bmp As Bitmap = pBox.Image
		Dim data As BitmapData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat)
		Marshal.Copy(imgData, 0, data.Scan0, imgData.Length - 1)
		bmp.UnlockBits(data)

		pBox.Invalidate()

		' Show timing results
		Dim seconds As Double = sw.ElapsedMilliseconds / 1000
		ShowStatus(lb, "Evaluations: {1:n0}{0}Time: {2:n2} seconds{0}Speed: {3:n0} evaluations/sec", _
		 Environment.NewLine, _
		 MyImageBitmapSize, seconds, _
		 MyImageBitmapSize / seconds)

	End Sub

	Private Sub SetExpressionFromTextbox(ByVal e As IGenericExpression(Of Double), ByVal tb As TextBox)

		If tb Is Me.tbRed Then
			MyRedExpression = e
		ElseIf tb Is Me.tbGreen Then
			MyGreenExpression = e
		Else
			MyBlueExpression = e
		End If

	End Sub

	Private Sub SetExpressionFromTextbox(ByVal t As Tabulator, ByVal tb As TextBox)

		If tb Is Me.tbRed Then
			MyRedTabulator = t
		ElseIf tb Is Me.tbGreen Then
			MyGreenTabulator = t
		Else
			MyBlueTabulator = t
		End If

	End Sub

	Private Sub CreateExpressionsFromPreset(ByVal preset As PresetInfo)

		Dim sw As New Stopwatch()
		sw.Start()

		MyRedExpression = MyContext.CompileGeneric(Of Double)(preset.MyRed)
		MyGreenExpression = MyContext.CompileGeneric(Of Double)(preset.MyGreen)
		MyBlueExpression = MyContext.CompileGeneric(Of Double)(preset.MyBlue)

		sw.Stop()

		ShowStatus(Me.lbStatus, "Presets compiled in {0:n0}ms", sw.ElapsedMilliseconds)

		sw = Stopwatch.StartNew()

		MyRedTabulator = MyILCalcContext.CreateTabulator(preset.MyRed)
		MyGreenTabulator = MyILCalcContext.CreateTabulator(preset.MyGreen)
		MyBlueTabulator = MyILCalcContext.CreateTabulator(preset.MyBlue)

		sw.Stop()

		ShowStatus(Me.lbStatus2, "Presets compiled in {0:n0}ms", sw.ElapsedMilliseconds)

	End Sub

	Private Sub CreateSingleExpression(ByVal source As TextBox)

		Dim expr As IGenericExpression(Of Double)

		Try

			Dim sw As New Stopwatch()
			sw.Start()
			' Try to create the expression
			expr = MyContext.CompileGeneric(Of Double)(source.Text)

			' If we get here, then the expression was compiled successfully
			sw.Stop()

			source.BackColor = Color.Empty
			ShowStatus(Me.lbStatus, "Expression compiled in {0:n0}ms", sw.ElapsedMilliseconds)

		Catch ex As ExpressionCompileException
			' Could not compile the exception so show error 
			ShowStatus(Me.lbStatus, ex.Message)
			expr = Nothing
			source.BackColor = Color.Tomato
		End Try

		Me.SetExpressionFromTextbox(expr, source)
		Me.cmdGenerate.Enabled = Not MyRedExpression Is Nothing And Not MyGreenExpression Is Nothing And Not MyBlueExpression Is Nothing

	End Sub

	Private Sub CreateSingleExpression2(ByVal source As TextBox)

		Dim tab As Tabulator

		Try
			Dim sw As New Stopwatch()
			sw.Start()

			tab = MyILCalcContext.CreateTabulator(source.Text)

			' If we get here, then the expression was compiled successfully
			sw.Stop()

			source.BackColor = Color.Empty
			ShowStatus(Me.lbStatus2, "Expression compiled in {0:n0}ms", sw.ElapsedMilliseconds)

		Catch ex As SyntaxException

			' Could not compile the exception so show error 
			ShowStatus(Me.lbStatus2, "{0}" + vbCrLf + "Column: {1}  Lenght: {2}", ex.Message, ex.Position, ex.Length)
			tab = Nothing
			source.BackColor = Color.Tomato

		End Try

		Me.SetExpressionFromTextbox(tab, source)
		Me.cmdGenerate.Enabled = _
		 Not MyRedTabulator Is Nothing And _
		 Not MyGreenTabulator Is Nothing And _
		 Not MyBlueTabulator Is Nothing

	End Sub

	Private Sub SaveImage()

		Dim sfd As New SaveFileDialog()
		sfd.Filter = "PNG files (*.png)|*.png"

		If sfd.ShowDialog(Me) = DialogResult.OK Then
			Dim bmp As Bitmap = Me.PictureBox1.Image
			bmp.Save(sfd.FileName, ImageFormat.Png)
		End If

		sfd.Dispose()

	End Sub

	Private Sub LoadPreset(ByVal preset As PresetInfo)

		Me.UnHookTextboxes()
		Me.tbRed.Text = preset.MyRed
		Me.tbGreen.Text = preset.MyGreen
		Me.tbBlue.Text = preset.MyBlue
		Me.HookTextboxes()
		Me.CreateExpressionsFromPreset(preset)
		Me.cmdGenerate.Enabled = True
		Me.tbRed.BackColor = Color.Empty
		Me.tbGreen.BackColor = Color.Empty
		Me.tbBlue.BackColor = Color.Empty

	End Sub

	Private Shared Sub ShowStatus(ByVal label As Label, _
	 ByVal msg As String, _
	 ByVal ParamArray args As Object())

		label.Text = String.Format(msg, args)

	End Sub

#Region "EventHandlers"

	Private Sub cmdGenerate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdGenerate.Click

		Thread.CurrentThread.Priority = ThreadPriority.Highest

		Me.Generate(Me.PictureBox1, MyImageData, AddressOf Me.GenerateImage, Me.lbStatus)
		Me.Generate(Me.PictureBox2, MyImageData2, AddressOf Me.GenerateImage2, Me.lbStatus2)

		Thread.CurrentThread.Priority = ThreadPriority.Normal

	End Sub

	' Save the generated image to a png file
	Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click

		Me.SaveImage()

	End Sub

	Private Sub ExpressionTextbox_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

		Me.CreateSingleExpression(sender)
		Me.CreateSingleExpression2(sender)

	End Sub

	Private Sub ddPresets_FirstSelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)

		RemoveHandler ddPresets.SelectedIndexChanged, AddressOf ddPresets_FirstSelectedIndexChanged
		Me.ddPresets.DropDownStyle = ComboBoxStyle.DropDownList

	End Sub

	Private Sub ddPresets_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddPresets.SelectedIndexChanged

		Dim preset As PresetInfo = Me.ddPresets.SelectedItem
		Me.LoadPreset(preset)

	End Sub

	Private Sub LinkLabel1_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked

		Process.Start(ORIGINAL_CONCEPT_URL)

	End Sub

#End Region


	Private Sub MainForm_SizeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.SizeChanged

		InitSizes()

		Me.PictureBox1.Image = CreateInitialImage()
		Me.PictureBox2.Image = CreateInitialImage()

		MyImageData = New Byte(MyImageBitmapSize) {}
		MyImageData2 = New Byte(MyImageBitmapSize) {}

	End Sub

End Class