Imports System.Windows
Imports System.Windows.Forms
Imports System.Windows.Input
Imports GlobalArcOpsFunctionality

Public Class ContentManagerMainInterface
    Private imageManagerUserControl As ImageManagerMainInterface
    Private audioManagerUserControl As AudioManagerMainInterface
    Private videoManagerUserControl As VideoManagerMainInterface
    Private fileManagerUserControl As FileManagerMainInterface

    Private Sub imageManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles imageManagerButton.Click
        If imageManagerUserControl Is Nothing Then
            imageManagerUserControl = New ImageManagerMainInterface
        End If
        pageContainer.Content = imageManagerUserControl
        toggleFieldVisibility()
    End Sub

    Private Sub toggleFieldVisibility()
        menuButtonGroup.Visibility = Visibility.Collapsed
        pageContainer.Visibility = Visibility.Visible
        'mainPagebutton.Visibility = Visibility.Visible
    End Sub

    Private Sub fileManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles fileManagerButton.Click
        If fileManagerUserControl Is Nothing Then
            fileManagerUserControl = New FileManagerMainInterface()
        End If
        pageContainer.Content = fileManagerUserControl
        toggleFieldVisibility()
    End Sub



    Private Sub videoManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles videoManagerButton.Click
        If videoManagerUserControl Is Nothing Then
            videoManagerUserControl = New VideoManagerMainInterface
        End If
        pageContainer.Content = videoManagerUserControl
        toggleFieldVisibility()
    End Sub

    Private Sub audioManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles audioManagerButton.Click
        If audioManagerUserControl Is Nothing Then
            audioManagerUserControl = New AudioManagerMainInterface
        End If
        pageContainer.Content = audioManagerUserControl
        toggleFieldVisibility()
    End Sub

    'Private Sub mainPagebutton_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles mainPagebutton.MouseLeftButtonUp
    '    pageContainer.Content = Nothing
    '    menuButtonGroup.Visibility = Visibility.Visible
    '    pageContainer.Visibility = Visibility.Collapsed
    '    mainPagebutton.Visibility = Visibility.Collapsed
    'End Sub
End Class
