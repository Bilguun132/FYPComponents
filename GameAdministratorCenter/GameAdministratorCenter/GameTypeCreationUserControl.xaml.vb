Imports System.Windows
Imports ContentManager
Imports GameAdministratorCenter.Contracts

Public Class GameTypeCreationUserControl
    Private selectedGameType As GAME_TYPE

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        gameTypeNameTextBox.Text = ""
        imageContainter.Content = Nothing
        addEditPopup.PlacementTarget = addButton
        addEditPopup.IsOpen = True
        selectedGameType = Nothing
    End Sub

    Private Sub addImageButton_Click(sender As Object, e As RoutedEventArgs) Handles addImageButton.Click
        Dim imageManager As New ImageManagerMainInterface
        imageManager.afterSelectFunctionDelegate = AddressOf updateSelectedImage
        PopupDialogFunctions.open_dialog(imageManager)
    End Sub

    Private Sub updateSelectedImage(selectedManagerID As Integer)
        Dim viewer = New ImageViewer(selectedManagerID)
        viewer.hideButtonPanel()
        imageContainter.Content = viewer
        PopupDialogFunctions.close_dialog()
        addEditPopup.IsOpen = True
    End Sub

    Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
        addEditPopup.IsOpen = False
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If selectedGameType IsNot Nothing Then
            selectedGameType.IS_DELETED = True
            getDatabaseEntity.SaveChanges()
            selectedGameType = Nothing
            editButton.IsEnabled = False
            deleteButton.IsEnabled = False
            loadGameTypeList()
        End If
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        If selectedGameType IsNot Nothing Then
            gameTypeNameTextBox.Text = selectedGameType.GAME_TYPE_NAME
            If selectedGameType.IMAGE_MANAGER_LINK_ID IsNot Nothing Then
                Dim viewer = New ImageViewer(Integer.Parse(selectedGameType.IMAGE_MANAGER_LINK_ID))
                viewer.hideButtonPanel()
                imageContainter.Content = viewer
            End If
            gameTypeDescTextBox.Text = selectedGameType.DESCRIPTION
            addEditPopup.PlacementTarget = editButton
            addEditPopup.IsOpen = True
        End If
    End Sub

    Private Sub GameTypeCreationUserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        loadGameTypeList()
    End Sub

    Private Sub loadGameTypeList()
        flowLayout.Children.Clear()
        Dim gameTypeList As List(Of GAME_TYPE) = (From query In getDatabaseEntity.GAME_TYPE Where query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False).ToList
        If gameTypeList IsNot Nothing Then
            For Each gameType In gameTypeList
                Dim gameTypeView As New GameTypeView(gameType)
                gameTypeView.gameTypeSelectedDelegate = AddressOf updateSelectedGameType
                flowLayout.Children.Add(gameTypeView)
            Next
        End If
    End Sub

    Private Sub saveButton_Click(sender As Object, e As RoutedEventArgs) Handles saveButton.Click
        If selectedGameType IsNot Nothing Then
            selectedGameType.GAME_TYPE_NAME = gameTypeNameTextBox.Text
            If imageContainter.Content IsNot Nothing Then
                Dim imageViewer = DirectCast(imageContainter.Content, ImageViewer)
                selectedGameType.IMAGE_MANAGER_LINK_ID = imageViewer.imageManager.ID
            End If
            selectedGameType.DESCRIPTION = gameTypeDescTextBox.Text
            getDatabaseEntity.SaveChanges()
            addEditPopup.IsOpen = False
        Else
            Dim newType As New GAME_TYPE
            newType.GAME_TYPE_NAME = gameTypeNameTextBox.Text
            If imageContainter.Content IsNot Nothing Then
                Dim imageViewer = DirectCast(imageContainter.Content, ImageViewer)
                newType.IMAGE_MANAGER_LINK_ID = imageViewer.imageManager.ID
            End If
            newType.DESCRIPTION = gameTypeDescTextBox.Text
            getDatabaseEntity.GAME_TYPE.Add(newType)
            getDatabaseEntity.SaveChanges()
            addEditPopup.IsOpen = False
        End If
        loadGameTypeList()
    End Sub

    Private Sub updateSelectedGameType(gameType As GAME_TYPE)
        selectedGameType = gameType
        If selectedGameType IsNot Nothing Then
            editButton.IsEnabled = True
            deleteButton.IsEnabled = True
        End If
    End Sub
End Class
