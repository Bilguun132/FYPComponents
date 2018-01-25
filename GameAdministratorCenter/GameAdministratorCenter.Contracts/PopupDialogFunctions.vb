Imports System.Windows
Imports System.Windows.Controls

Public Module PopupDialogFunctions
    Private windowStack As New Stack(Of Window)

    Public Sub open_dialog(userControl As UserControl)
        Dim newWindow As New Window
        newWindow.Content = userControl
        newWindow.WindowState = WindowState.Maximized
        newWindow.Activate()
        newWindow.Show()
        windowStack.Push(newWindow)

    End Sub

    Public Sub close_dialog()
        If windowStack.Count > 0 Then
            Dim windowToClose As Window = windowStack.Pop
            windowToClose.Close()
        End If
    End Sub
End Module
