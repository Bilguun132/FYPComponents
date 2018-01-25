Imports System.IO
Imports System.Windows
Imports System.Windows.Forms
Imports DevExpress.Xpf.Grid
Imports GameAdministratorCenter.Contracts

Public Class FileManagerMainInterface
    Public Delegate Sub afterSelectFunction(selectedManagerID As Integer)
    Public afterSelectFunctionDelegate As afterSelectFunction = Nothing
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing
    Private tempFileName As String = ""
    Private viewMode As ManagerViewMode

    Private Sub FileManagerMainInterface_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        viewMode = viewMode.defaultVersions
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
        populateFileManagerList()
    End Sub

    Private Sub populateFileManagerList()
        Dim pulledFileManagerList As List(Of CONTENT_MANAGER)
        If viewMode = viewMode.defaultVersions Then
            pulledFileManagerList = getAllDefaultManagers(ContentManagerType.GeneralFile)
        Else
            pulledFileManagerList = getAllManagers(ContentManagerType.GeneralFile)
        End If
        fileManagerGridControl.ItemsSource = pulledFileManagerList
        fileManagerGridControl.SelectedItem = Nothing
        fileManagerGridView.BestFitColumns()
        fileManagerGridControl.UnselectAll()
        detailPage.Content = Nothing
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        Dim newFileManager As New CONTENT_MANAGER
        newFileManager.IS_DEFAULT = True
        newFileManager.IS_DELETED = False
        newFileManager.CONTENT_MANAGER_TYPE = ContentManagerType.GeneralFile
        Dim newFileManagerDetailPage As New ContentManagerDetailPage(newFileManager, ContentManagerDetailPage.ViewMode.edit, ContentManagerType.GeneralFile)
        newFileManagerDetailPage.hideVersionBrowserGroup()
        newFileManagerDetailPage.hideSetDefaultVersionButton()
        newFileManagerDetailPage.afterCloseDelegate = AddressOf PopupDialogFunctions.close_dialog
        newFileManagerDetailPage.afterSaveDelegate = AddressOf updateMainGridControl
        open_popup_with_max_size(newFileManagerDetailPage)
    End Sub

    Private Sub updateMainGridControl(passedInManager As CONTENT_MANAGER)
        PopupDialogFunctions.close_dialog()
        Dim pulledFileManagerList As List(Of CONTENT_MANAGER) = getAllDefaultManagers(ContentManagerType.GeneralFile)
        fileManagerGridControl.ItemsSource = pulledFileManagerList
        fileManagerGridView.BestFitColumns()
        fileManagerGridView.MoveLastRow()
        fileManagerGridControl.SelectItem(pulledFileManagerList.Count - 1)
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim messageBoxAnswer = Forms.MessageBox.Show("Confirm delete? All subversions will be deleted as well", "Delete Confirmation", MessageBoxButtons.OKCancel)
            If messageBoxAnswer = DialogResult.OK Then
                Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
                Dim errorMessage As String = deleteManagerAndResetRoot(detailPageUserControl.currentManager)
                If errorMessage = "" Then
                    populateFileManagerList()
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

    Private Sub FileManagerGridControl_SelectedItemChanged(sender As Object, e As SelectedItemChangedEventArgs) Handles fileManagerGridControl.SelectedItemChanged
        loadCurrentlySelectedCluster()
    End Sub

    Private Sub loadCurrentlySelectedCluster()
        If fileManagerGridControl.SelectedItem IsNot Nothing Then
            Dim selectedManagerEntity As CONTENT_MANAGER = DirectCast(fileManagerGridControl.SelectedItem, CONTENT_MANAGER)
            Dim FileManagerDetailView As New ContentManagerDetailPage(selectedManagerEntity, ContentManagerDetailPage.ViewMode.view, ContentManagerType.GeneralFile)
            detailPage.Content = FileManagerDetailView
            ToggleViewAndAdobeButton()
        End If
    End Sub

    Private Sub ToggleViewAndAdobeButton()
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            Dim currentlySelectedManager As CONTENT_MANAGER = detailPageUserControl.currentManager
            If currentlySelectedManager IsNot Nothing AndAlso currentlySelectedManager.ID <> 0 Then
                Dim fileStorageEntity As FILE_STORAGE = currentlySelectedManager.FILE_STORAGE
                If fileStorageEntity IsNot Nothing AndAlso fileStorageEntity.FILE_NAME IsNot Nothing Then
                    Dim extension As String = IO.Path.GetExtension(fileStorageEntity.FILE_NAME).ToLower()
                    If extension = ".pdf" Or extension = ".xls" Or extension = ".xlsx" Then
                        viewButton.IsEnabled = True
                        viewButton.Opacity = 1
                        If extension = ".pdf" Then
                            viewInAdobeButton.IsEnabled = True
                            viewInAdobeButton.Opacity = 1
                        Else
                            viewInAdobeButton.IsEnabled = False
                            viewInAdobeButton.Opacity = 0.3
                        End If
                    Else
                        viewButton.IsEnabled = False
                        viewButton.Opacity = 0.3
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub viewButton_Click(sender As Object, e As RoutedEventArgs) Handles viewButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            Dim currentlySelectedManager As CONTENT_MANAGER = detailPageUserControl.currentManager
            If currentlySelectedManager IsNot Nothing AndAlso currentlySelectedManager.ID <> 0 Then
                Dim fileStorageEntity As FILE_STORAGE = currentlySelectedManager.FILE_STORAGE
                If fileStorageEntity IsNot Nothing AndAlso fileStorageEntity.FILE_NAME IsNot Nothing Then
                    Dim extension As String = IO.Path.GetExtension(fileStorageEntity.FILE_NAME).ToLower()
                    If extension = ".pdf" Then
                        Dim pdfViewer As New PDFViewer(currentlySelectedManager.ID)
                        pdfViewer.afterCloseFunctionDelegate = AddressOf PopupDialogFunctions.close_dialog
                        open_popup_with_max_size(pdfViewer)
                    ElseIf extension = ".xls" Or extension = ".xlsx" Then
                        Dim excel_viewer As New ExcelViewer(currentlySelectedManager.ID)
                        excel_viewer.DelegateCloseFunction = AddressOf PopupDialogFunctions.close_dialog
                        open_popup_with_max_size(excel_viewer)
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
                downloadContentByManagerId(currentlySelectedManager.ID, ContentManagerType.GeneralFile)
            End If
        End If
    End Sub

    'Private Sub viewAllButton_Click(sender As Object, e As RoutedEventArgs) Handles viewAllButton.Click
    '    viewMode = viewMode.allVersions
    '    populateFileManagerList()
    'End Sub

    'Private Sub viewDefaultButton_Click(sender As Object, e As RoutedEventArgs) Handles viewDefaultButton.Click
    '    viewMode = viewMode.defaultVersions
    '    populateFileManagerList()
    'End Sub



    Private Sub viewInAdobeButton_Click(sender As Object, e As RoutedEventArgs) Handles viewInAdobeButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            Dim currentlySelectedManager As CONTENT_MANAGER = detailPageUserControl.currentManager
            Dim mediaManager As CONTENT_MANAGER = getContentManager(currentlySelectedManager.ID)
            If mediaManager IsNot Nothing Then
                Dim associatedFileStorage As FILE_STORAGE = mediaManager.FILE_STORAGE
                If associatedFileStorage IsNot Nothing AndAlso associatedFileStorage.FILE_STRING IsNot Nothing Then
                    Try
                        loadPdfString(associatedFileStorage.FILE_STRING)
                        Process.Start(tempFileName)
                    Catch ex As Exception
                        MsgBox(ex.Message)
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub loadPdfString(inputString As String)
        tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf")
        Dim mediaByteArray As Byte() = Convert.FromBase64String(inputString)
        File.WriteAllBytes(tempFileName, mediaByteArray)
    End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        If afterCloseFunctionDelegate IsNot Nothing Then
            afterCloseFunctionDelegate()
        End If
    End Sub

    Private Sub fileManagerGridControl_CustomUnboundColumnData(sender As Object, e As GridColumnDataEventArgs)
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
End Class
