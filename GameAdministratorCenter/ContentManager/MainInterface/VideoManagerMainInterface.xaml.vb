Imports System.Windows
Imports System.Windows.Forms
Imports DevExpress.Xpf.Grid
Imports GameAdministratorCenter.Contracts

Public Class VideoManagerMainInterface

    Public Delegate Sub afterSelectFunction(selectedManagerID As Integer)
    Public afterSelectFunctionDelegate As afterSelectFunction = Nothing
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing

    Private viewMode As ManagerViewMode

    Private Sub VideoManagerMainInterface_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        viewMode = viewMode.defaultVersions
        populateVideoManagerList()
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

    Private Sub populateVideoManagerList()
        Dim pulledVideoManagerList As List(Of CONTENT_MANAGER)
        If viewMode = viewMode.defaultVersions Then
            pulledVideoManagerList = getAllDefaultManagers(ContentManagerType.Video)
        Else
            pulledVideoManagerList = getAllManagers(ContentManagerType.Video)
        End If
        videoManagerGridControl.ItemsSource = pulledVideoManagerList
        videoManagerGridView.BestFitColumns()
        videoManagerGridControl.SelectedItem = Nothing
        videoManagerGridControl.UnselectAll()
        detailPage.Content = Nothing
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        Dim newVideoManager As New CONTENT_MANAGER
        newVideoManager.IS_DEFAULT = True
        newVideoManager.IS_DELETED = False
        newVideoManager.CONTENT_MANAGER_TYPE = ContentManagerType.Video
        Dim newVideoManagerDetailPage As New ContentManagerDetailPage(newVideoManager, ContentManagerDetailPage.ViewMode.edit, ContentManagerType.Video)
        newVideoManagerDetailPage.hideVersionBrowserGroup()
        newVideoManagerDetailPage.hideSetDefaultVersionButton()
        newVideoManagerDetailPage.afterCloseDelegate = AddressOf PopupDialogFunctions.close_dialog
        newVideoManagerDetailPage.afterSaveDelegate = AddressOf updateMainGridControl
        open_popup_with_max_size(newVideoManagerDetailPage)
    End Sub

    Private Sub updateMainGridControl(passedInManager As CONTENT_MANAGER)
        PopupDialogFunctions.close_dialog()
        Dim pulledVideoManagerList As List(Of CONTENT_MANAGER) = getAllDefaultManagers(ContentManagerType.Video)
        videoManagerGridControl.ItemsSource = pulledVideoManagerList
        videoManagerGridView.BestFitColumns()
        videoManagerGridView.MoveLastRow()

        videoManagerGridControl.SelectItem(pulledVideoManagerList.Count - 1)
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim messageBoxAnswer = Forms.MessageBox.Show("Confirm delete? All subversions will be deleted as well", "Delete Confirmation", MessageBoxButtons.OKCancel)
            If messageBoxAnswer = DialogResult.OK Then
                Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
                Dim errorMessage As String = deleteManagerAndResetRoot(detailPageUserControl.currentManager)
                If errorMessage = "" Then
                    populateVideoManagerList()
                    Forms.MessageBox.Show("Save Successful")
                Else
                    Forms.MessageBox.Show("Save Failed")
                End If
            End If
        End If
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            detailPageUserControl.setView(ContentManagerDetailPage.ViewMode.edit)
        End If
    End Sub

    Private Sub videoManagerGridControl_SelectedItemChanged(sender As Object, e As SelectedItemChangedEventArgs) Handles videoManagerGridControl.SelectedItemChanged
        loadCurrentlySelectedCluster()
    End Sub

    Private Sub loadCurrentlySelectedCluster()
        If videoManagerGridControl.SelectedItem IsNot Nothing Then
            Dim selectedManagerEntity As CONTENT_MANAGER = DirectCast(videoManagerGridControl.SelectedItem, CONTENT_MANAGER)
            Dim videoManagerDetailView As New ContentManagerDetailPage(selectedManagerEntity, ContentManagerDetailPage.ViewMode.view, ContentManagerType.Video)
            detailPage.Content = videoManagerDetailView
        End If
    End Sub

    '<!--The fieldname for the unbound column must be different from all the existing fieldnames in the source for the event CustomUnboundcolumnData to be fired-->
    Private Sub videoManagerGridControl_CustomUnboundColumnData(sender As Object, e As GridColumnDataEventArgs)
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

    '    view_button_clicked()

    'End Sub

    Private Sub view_button_clicked()
        If videoManagerGridControl.SelectedItem IsNot Nothing Then
            Dim selected_item As CONTENT_MANAGER = TryCast(videoManagerGridControl.SelectedItem, CONTENT_MANAGER)
            If selected_item IsNot Nothing Then
                openVideoView(selected_item.ID)
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
                downloadContentByManagerId(currentlySelectedManager.ID, ContentManagerType.Video)
            End If
        End If
    End Sub

    'Private Sub viewAllButton_Click(sender As Object, e As RoutedEventArgs) Handles viewAllButton.Click
    '    viewMode = viewMode.allVersions
    '    populateVideoManagerList()
    'End Sub

    'Private Sub viewDefaultButton_Click(sender As Object, e As RoutedEventArgs) Handles viewDefaultButton.Click
    '    viewMode = viewMode.defaultVersions
    '    populateVideoManagerList()
    'End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        If afterCloseFunctionDelegate IsNot Nothing Then
            afterCloseFunctionDelegate()
        End If
    End Sub

End Class
