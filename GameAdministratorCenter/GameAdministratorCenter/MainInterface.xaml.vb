Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports ContentManager
Imports GameAdministratorCenter.Contracts

Public Class MainInterface
    Private Sub contentManageManagementButton_Click(sender As Object, e As RoutedEventArgs) Handles contentManageManagementButton.Click
        Dim mainInterface As New ContentManagerMainInterface
        ShowThisUserControlCallback(mainInterface)
    End Sub

    Private Sub gameCreationButton_Click(sender As Object, e As RoutedEventArgs) Handles gameCreationButton.Click
        Dim gameCreationInterface As New GameCreationInterface
        PublicFunctions.ShowThisUserControlCallback(gameCreationInterface)
    End Sub

    Private Sub gamePlayManagementButton_Click(sender As Object, e As RoutedEventArgs) Handles gamePlayManagementButton.Click

    End Sub

    Private Sub MainInterface_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        PublicFunctions.ShowThisUserControlCallback = AddressOf ShowThisUserControl
    End Sub

    Private Sub ShowThisUserControl(ByVal userControl As Object)
        If userControl Is Nothing Then
            ShowButtons()
        Else
            ShowUserControl(userControl)
        End If
    End Sub

    Private Sub ShowUserControl(userControl As UserControl)
        buttonGroupContainer.Visibility = Visibility.Collapsed
        layoutItemContainer.Content = userControl
        layoutItemContainer.Visibility = Visibility.Visible
    End Sub

    Private Sub ShowButtons()
        buttonGroupContainer.Visibility = Visibility.Visible
        layoutItemContainer.Visibility = Visibility.Collapsed
    End Sub

    Private Sub mainPagebutton_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles mainPagebutton.MouseLeftButtonUp
        PublicFunctions.ShowThisUserControlCallback(Nothing)
    End Sub

End Class
