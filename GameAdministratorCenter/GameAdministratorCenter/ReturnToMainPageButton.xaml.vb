Imports System.Windows
Imports System.Windows.Input

Public Class ReturnToMainPageButton
    Private Sub MainMenuButtonControl_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonDown
        Me.Background = System.Windows.Media.Brushes.SteelBlue
    End Sub

    Private Sub MainMenuButtonControl_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        Me.Background = System.Windows.Media.Brushes.LightSteelBlue
    End Sub

    Private Sub ReturnToMainPageButton_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Me.Background = System.Windows.Media.Brushes.LightSteelBlue
    End Sub
End Class
