Module App

	<STAThread()> _
	Public Sub Main(ByVal args As String())

		Application.EnableVisualStyles()
		Application.SetCompatibleTextRenderingDefault(False)
		Application.Run(New MainForm())

	End Sub

End Module