Class MainWindow
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim mainInterface As New MainInterface
        mainLayoutItem.Children.Add(mainInterface)
    End Sub
End Class
