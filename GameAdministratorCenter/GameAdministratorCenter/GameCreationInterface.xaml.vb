Imports System.ComponentModel
Imports System.Windows
Imports GameAdministratorCenter.Contracts

Public Class GameCreationInterface
    Private Sub createGameButton_Click(sender As Object, e As RoutedEventArgs) Handles createGameButton.Click
        mainLayoutItem.Content = New GameManagerUserControl
    End Sub

    Private Sub editGameButton_Click(sender As Object, e As RoutedEventArgs) Handles editGameButton.Click
        mainLayoutItem.Content = New FirmListUserControl()
    End Sub

    Private Sub editMarketButton_Click(sender As Object, e As RoutedEventArgs) Handles editMarketButton.Click
        mainLayoutItem.Content = New EditMarketInfoUserControl()
    End Sub

    Private Sub saveButton_Click(sender As Object, e As System.Windows.RoutedEventArgs)
        DatabaseEntitySingleton.getDatabaseEntity.SaveChanges()
    End Sub
End Class
