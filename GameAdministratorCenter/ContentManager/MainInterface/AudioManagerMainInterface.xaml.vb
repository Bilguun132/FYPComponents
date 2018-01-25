Imports System.Windows
Imports System.Windows.Forms
Imports DevExpress.Xpf.Grid
Imports GameAdministratorCenter.Contracts


Public Class AudioManagerMainInterface
    Public Delegate Sub afterSelectFunction(selectedManagerID As Integer)
    Public afterSelectFunctionDelegate As afterSelectFunction = Nothing
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing

    Private viewMode As ManagerViewMode

    Private Sub AudioManagerMainInterface_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        viewMode = viewMode.defaultVersions
        populateAudioManagerList()
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

    Private Sub populateAudioManagerList()
        Dim pulledAudioManagerList As List(Of CONTENT_MANAGER)
        If viewMode = viewMode.defaultVersions Then
            pulledAudioManagerList = getAllDefaultManagers(ContentManagerType.Audio)
        Else
            pulledAudioManagerList = getAllManagers(ContentManagerType.Audio)
        End If
        audioManagerGridControl.ItemsSource = pulledAudioManagerList
        audioManagerGridView.BestFitColumns()
        audioManagerGridControl.SelectedItem = Nothing
        audioManagerGridControl.UnselectAll()
        detailPage.Content = Nothing
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        Dim newAudioManager As New CONTENT_MANAGER
        newAudioManager.IS_DEFAULT = True
        newAudioManager.IS_DELETED = False
        newAudioManager.CONTENT_MANAGER_TYPE = ContentManagerType.Audio
        Dim newAudioManagerDetailPage As New ContentManagerDetailPage(newAudioManager, ContentManagerDetailPage.ViewMode.edit, ContentManagerType.Audio)
        newAudioManagerDetailPage.hideVersionBrowserGroup()
        newAudioManagerDetailPage.hideSetDefaultVersionButton()
        newAudioManagerDetailPage.afterCloseDelegate = AddressOf PopupDialogFunctions.close_dialog
        newAudioManagerDetailPage.afterSaveDelegate = AddressOf updateMainGridControl

        open_popup_with_max_size(newAudioManagerDetailPage)
    End Sub

    Private Sub updateMainGridControl(passedInManager As CONTENT_MANAGER)
        PopupDialogFunctions.close_dialog()
        Dim pulledAudioManagerList As List(Of CONTENT_MANAGER) = getAllDefaultManagers(ContentManagerType.Audio)
        audioManagerGridControl.ItemsSource = pulledAudioManagerList
        audioManagerGridView.BestFitColumns()
        audioManagerGridView.MoveLastRow()
        audioManagerGridControl.SelectItem(pulledAudioManagerList.Count - 1)
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If detailPage.Content IsNot Nothing Then
            Dim messageBoxAnswer = Forms.MessageBox.Show("Confirm delete? All subversions will be deleted as well", "Delete Confirmation", MessageBoxButtons.OKCancel)
            If messageBoxAnswer = DialogResult.OK Then
                Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
                Dim errorMessage As String = deleteManagerAndResetRoot(detailPageUserControl.currentManager)
                If errorMessage = "" Then
                    populateAudioManagerList()
                    Forms.MessageBox.Show("Save Successful")
                Else
                    Forms.MessageBox.Show("Save Failed")
                End If
            End If
        End If
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        'If AudioManagerGridControl.SelectedItem IsNot Nothing Then
        '    Dim selectedAudioManager As AudioManagerDTO = DirectCast(AudioManagerGridControl.SelectedItem, AudioManagerDTO)
        '    Dim selectedManagerEntity As CONTENT_MANAGER = helperService.getContentManager(selectedAudioManager.ID)
        '    Dim AudioManagerDetailView As New AudioManagerDetailPage(selectedManagerEntity, AudioManagerDetailPage.ViewMode.edit)
        '    detailPage.Content = AudioManagerDetailView
        'End If
        If detailPage.Content IsNot Nothing Then
            Dim detailPageUserControl As ContentManagerDetailPage = DirectCast(detailPage.Content, ContentManagerDetailPage)
            detailPageUserControl.setView(ContentManagerDetailPage.ViewMode.edit)
        End If
    End Sub

    Private Sub AudioManagerGridControl_SelectedItemChanged(sender As Object, e As SelectedItemChangedEventArgs) Handles audioManagerGridControl.SelectedItemChanged
        loadCurrentlySelectedCluster()
    End Sub

    Private Sub loadCurrentlySelectedCluster()
        If audioManagerGridControl.SelectedItem IsNot Nothing Then
            Dim selectedManagerEntity As CONTENT_MANAGER = DirectCast(audioManagerGridControl.SelectedItem, CONTENT_MANAGER)
            Dim AudioManagerDetailView As New ContentManagerDetailPage(selectedManagerEntity, ContentManagerDetailPage.ViewMode.view, ContentManagerType.Audio)
            detailPage.Content = AudioManagerDetailView
        End If
    End Sub

    'Private Sub viewButton_Click(sender As Object, e As RoutedEventArgs) Handles viewButton.Click
    '    If audioManagerGridControl.SelectedItem IsNot Nothing Then
    '        Dim selected_audio As CONTENT_MANAGER = TryCast(audioManagerGridControl.SelectedItem, CONTENT_MANAGER)
    '        If selected_audio IsNot Nothing Then
    '            openAudioView(selected_audio.ID)
    '        End If
    '    End If

    'End Sub

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
                downloadContentByManagerId(currentlySelectedManager.ID, ContentManagerType.Audio)
            End If
        End If
    End Sub

    'Private Sub viewAllButton_Click(sender As Object, e As RoutedEventArgs) Handles viewAllButton.Click
    '    viewMode = viewMode.allVersions
    '    populateAudioManagerList()
    'End Sub

    'Private Sub viewDefaultButton_Click(sender As Object, e As RoutedEventArgs) Handles viewDefaultButton.Click
    '    viewMode = viewMode.defaultVersions
    '    populateAudioManagerList()
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
