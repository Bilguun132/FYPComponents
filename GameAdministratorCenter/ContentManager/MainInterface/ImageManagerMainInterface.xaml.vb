Imports System.Windows
Imports System.Windows.Forms
Imports DevExpress.Xpf.Grid
Imports GameAdministratorCenter.Contracts

Public Class ImageManagerMainInterface
    Public Delegate Sub afterSelectFunction(selectedManagerID As Integer)
    Public afterSelectFunctionDelegate As afterSelectFunction = Nothing
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing

    Private viewMode As ManagerViewMode

    Private Sub ImageManagerMainInterface_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        viewMode = ManagerViewMode.defaultVersions
        populateImageManagerList()
        If afterCloseFunctionDelegate IsNot Nothing Then
            closeButton.Visibility = Visibility.Visible
        Else
            closeButton.Visibility = Visibility.Collapsed
        End If

        If afterSelectFunctionDelegate IsNot Nothing Then
            selectButton.Visibility = Visibility.Visible
        Else
            selectButton.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub populateImageManagerList()
        Dim pulledImageManagerList As List(Of CONTENT_MANAGER)
        If viewMode = ManagerViewMode.defaultVersions Then
            pulledImageManagerList = getAllDefaultManagers(ContentManagerType.Image)
        Else
            pulledImageManagerList = getAllManagers(ContentManagerType.Image)
        End If
        imageManagerGridControl.ItemsSource = pulledImageManagerList
        imageManagerGridView.BestFitColumns()
        imageManagerGridControl.SelectedItem = Nothing
        imageManagerGridControl.UnselectAll()
        detailPage.Content = Nothing
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        Dim newImageManager As New CONTENT_MANAGER
        newImageManager.IS_DEFAULT = True
        newImageManager.IS_DELETED = False
        newImageManager.CONTENT_MANAGER_TYPE = ContentManagerType.Image
        Dim newImageManagerDetailPage As New ContentManagerDetailPage(newImageManager, ContentManagerDetailPage.ViewMode.edit, ContentManagerType.Image)
        newImageManagerDetailPage.hideVersionBrowserGroup()
        newImageManagerDetailPage.hideSetDefaultVersionButton()
        newImageManagerDetailPage.afterCloseDelegate = AddressOf PopupDialogFunctions.close_dialog
        newImageManagerDetailPage.afterSaveDelegate = AddressOf updateMainGridControl

        open_popup_with_max_size(newImageManagerDetailPage)
    End Sub

    Private Sub updateMainGridControl(passedInManager As CONTENT_MANAGER)
        PopupDialogFunctions.close_dialog()
        Dim pulledImageManagerList As List(Of CONTENT_MANAGER) = getAllDefaultManagers(ContentManagerType.Image)
        imageManagerGridControl.ItemsSource = pulledImageManagerList
        imageManagerGridView.BestFitColumns()
        imageManagerGridView.MoveLastRow()
        imageManagerGridControl.SelectItem(pulledImageManagerList.Count - 1)
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim messageBoxAnswer = Forms.MessageBox.Show("Confirm delete? All subversions will be deleted as well", "Deletion Confirmation", MessageBoxButtons.OKCancel)
            If messageBoxAnswer = DialogResult.OK Then
                Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
                Dim errorMessage As String = deleteManagerAndResetRoot(detailPageUserControl.currentManager)
                If errorMessage = "" Then
                    populateImageManagerList()
                    Forms.MessageBox.Show("Save Successful")
                Else
                    Forms.MessageBox.Show("Save Failed")
                End If
            End If
        End If
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        'If imageManagerGridControl.SelectedItem IsNot Nothing Then
        '    Dim selectedImageManager As ImageManagerDTO = DirectCast(imageManagerGridControl.SelectedItem, ImageManagerDTO)
        '    Dim selectedManagerEntity As CONTENT_MANAGER = helperService.getContentManager(selectedImageManager.ID)
        '    Dim imageManagerDetailView As New ImageManagerDetailPage(selectedManagerEntity, ImageManagerDetailPage.ViewMode.edit)
        '    detailPage.Content = imageManagerDetailView
        'End If
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            detailPageUserControl.setView(ContentManagerDetailPage.ViewMode.edit)
        End If
    End Sub

    Private Sub imageManagerGridControl_SelectedItemChanged(sender As Object, e As SelectedItemChangedEventArgs) Handles imageManagerGridControl.SelectedItemChanged
        loadCurrentlySelectedCluster()
    End Sub

    Private Sub loadCurrentlySelectedCluster()
        If imageManagerGridControl.SelectedItem IsNot Nothing Then
            Dim selectedManagerEntity As CONTENT_MANAGER = DirectCast(imageManagerGridControl.SelectedItem, CONTENT_MANAGER)
            Dim imageManagerDetailView As New ContentManagerDetailPage(selectedManagerEntity, ContentManagerDetailPage.ViewMode.view, ContentManagerType.Image)
            detailPage.Content = imageManagerDetailView
        End If
    End Sub

    '<!--The fieldname for the unbound column must be different from all the existing fieldnames in the source for the event CustomUnboundcolumnData to be fired-->
    Private Sub imageManagerGridControl_CustomUnboundColumnData(sender As Object, e As GridColumnDataEventArgs)
        If e.IsGetData Then
            If e.Column.FieldName = "THUMBNAIL" Then
                Dim currentManager As CONTENT_MANAGER = getContentManager(e.GetListSourceFieldValue("ID"))
                If currentManager IsNot Nothing Then
                    Dim controlImage As Controls.Image = convertHexStringToControlImage(currentManager.CONTENT_THUMBNAIL)
                    If controlImage IsNot Nothing Then
                        e.Value = controlImage.Source
                    End If
                End If
            End If
        End If
    End Sub

    'Private Sub viewButton_Click(sender As Object, e As RoutedEventArgs) Handles viewButton.Click
    '    If imageManagerGridControl.SelectedItem IsNot Nothing Then
    '        Dim selected_item As CONTENT_MANAGER = TryCast(imageManagerGridControl.SelectedItem, CONTENT_MANAGER)
    '        If selected_item IsNot Nothing Then
    '            openImageView(selected_item.ID)
    '        End If
    '    End If
    'End Sub

    Private Sub printButton_Click(sender As Object, e As RoutedEventArgs) Handles printButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            Dim currentlySelectedManager As CONTENT_MANAGER = detailPageUserControl.currentManager
            If currentlySelectedManager IsNot Nothing AndAlso currentlySelectedManager.ID <> 0 Then
                If currentlySelectedManager.IMAGE_STRING IsNot Nothing AndAlso currentlySelectedManager.IMAGE_STRING.IMAGE_HEX_STRING IsNot Nothing Then
                    Dim controlsImage As Controls.Image = convertHexStringToControlImage(currentlySelectedManager.IMAGE_STRING.IMAGE_HEX_STRING)
                    Dim drawingImage As Drawing.Image = convertControlImageToDrawingImage(controlsImage)
                    If drawingImage IsNot Nothing Then
                        Dim newDrawingImageList As New List(Of Drawing.Image)
                        newDrawingImageList.Add(drawingImage)

                        Dim newCustomPrinter As New CustomPrinter.PublicFunctions
                        newCustomPrinter.print_images(newDrawingImageList)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub selectButton_Click(sender As Object, e As RoutedEventArgs) Handles selectButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            Dim currentlySelectedManager As CONTENT_MANAGER = detailPageUserControl.currentManager
            If currentlySelectedManager IsNot Nothing AndAlso currentlySelectedManager.ID <> 0 AndAlso afterSelectFunctionDelegate IsNot Nothing Then
                afterSelectFunctionDelegate(currentlySelectedManager.ID)
            End If
        End If
    End Sub

    Private Sub downloadButton_Click(sender As Object, e As RoutedEventArgs) Handles downloadButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            Dim currentlySelectedManager As CONTENT_MANAGER = detailPageUserControl.currentManager
            If currentlySelectedManager IsNot Nothing AndAlso currentlySelectedManager.ID <> 0 Then
                downloadContentByManagerId(currentlySelectedManager.ID, ContentManagerType.Image)
            End If
        End If
    End Sub

    'Private Sub viewAllButton_Click(sender As Object, e As RoutedEventArgs) Handles viewAllButton.Click
    '    viewMode = ManagerViewMode.allVersions
    '    populateImageManagerList()
    'End Sub

    'Private Sub viewDefaultButton_Click(sender As Object, e As RoutedEventArgs) Handles viewDefaultButton.Click
    '    viewMode = ManagerViewMode.defaultVersions
    '    populateImageManagerList()
    'End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        If afterCloseFunctionDelegate IsNot Nothing Then
            afterCloseFunctionDelegate()
        End If
    End Sub

    Public Sub hideCloseButton()
        closeButton.Visibility = Visibility.Collapsed
    End Sub

    Public Sub showCloseButton()
        closeButton.Visibility = Visibility.Visible
    End Sub
End Class
