Imports System.Windows
Imports System.Windows.Input
Imports ContentManager
Imports DevExpress.Xpf.Editors
Imports GameAdministratorCenter.Contracts

Public Class AddEditGameUserControl
    Public Property selectedGame As GAME
    Private Property gameType As GAME_TYPE
    Private availableUserList As List(Of USER)
    Dim entities As FYP_DATABASEEntities = getDatabaseEntity

    Public Sub New(passedInId As Integer?, passedInGameType As GAME_TYPE)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        If passedInId IsNot Nothing Then
            selectedGame = (From query In getDatabaseEntity.GAMEs Where query.ID = passedInId).FirstOrDefault
            If selectedGame IsNot Nothing Then
                loadPage()
            End If
        Else
            selectedGame = New GAME
            selectedGame.IS_DELETED = False
            periodGridControl.ItemsSource = New List(Of PERIOD)
            userGridControl.ItemsSource = New List(Of USER)
            availableUserList = (From query In getDatabaseEntity.USERs Where query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False).ToList
            userSelectCombobox.ItemsSource = availableUserList
        End If
        gameType = passedInGameType
    End Sub

    Private Sub loadPage()
        gameNameTextBox.Text = selectedGame.GAME_NAME
        gamePasswordTextBox.Text = selectedGame.GAME_PASSWORD
        If selectedGame.IMAGE_MANAGER_LINK_ID IsNot Nothing Then
            updateSelectedImage(selectedGame.IMAGE_MANAGER_LINK_ID)
        End If
        gameDescTextBox.Text = selectedGame.DESCRIPTION
        Dim periodList As List(Of PERIOD) = (From query In getDatabaseEntity.PERIODs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                         query.GAME_LINK_ID = selectedGame.ID).ToList
        periodGridControl.ItemsSource = periodList
        periodGridView.BestFitColumns()

        Dim userList As List(Of USER) = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                   query.GAME_LINK_ID = selectedGame.ID Select query.USER).ToList
        If userList IsNot Nothing Then
            userList = userList.Distinct.ToList
        End If
        userGridControl.ItemsSource = userList
        userGridView.BestFitColumns()

        availableUserList = (From query In getDatabaseEntity.USERs Where query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False).ToList
        availableUserList = availableUserList.FindAll(Function(x)
                                                          If userList.Find(Function(y)
                                                                               If y.ID = x.ID Then
                                                                                   Return True
                                                                               Else
                                                                                   Return False
                                                                               End If
                                                                           End Function) Is Nothing Then
                                                              Return True
                                                          Else
                                                              Return False
                                                          End If
                                                      End Function).ToList
        userSelectCombobox.ItemsSource = availableUserList

        Dim availableFirms As List(Of GAME_FIRM) = selectedGame.GAME_FIRM.ToList
        firmAssignComboBox.ItemsSource = availableFirms
        firmAssignComboBox.DisplayMember = "FIRM_NAME"
        firmAssignComboBox.ValueMember = "ID"
    End Sub

    Friend Sub SaveChanges()
        If selectedGame.ID = 0 Then
            getDatabaseEntity.GAMEs.Add(selectedGame)
        End If

        selectedGame.GAME_NAME = gameNameTextBox.Text
        If imageContainer.Content IsNot Nothing Then
            Dim viewer = DirectCast(imageContainer.Content, ImageViewer)
            selectedGame.IMAGE_MANAGER_LINK_ID = viewer.imageManager.ID
        End If

        selectedGame.GAME_PASSWORD = gamePasswordTextBox.Text
        selectedGame.DESCRIPTION = gameDescTextBox.Text
        selectedGame.GAME_TYPE = gameType

        Dim periodList As List(Of PERIOD) = DirectCast(periodGridControl.ItemsSource, List(Of PERIOD))
        Dim periodNumber As Integer = 1
        For Each period In periodList
            If period.ID = 0 Then
                getDatabaseEntity.PERIODs.Add(period)
            End If
            period.GAME = selectedGame
            period.PERIOD_NUMBER = periodNumber
            periodNumber += 1
            period.IS_DELETED = False
        Next

        If selectedGame.ID < 0 Then
            Dim existingRelationship As List(Of USER_USER_ROLE_GAME_FIRM_RELATIONSHIP) = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where query.GAME_LINK_ID = selectedGame.ID).ToList
            For Each item In existingRelationship
                item.IS_DELETED = True
            Next
        End If
        Dim userList As List(Of USER) = DirectCast(userGridControl.ItemsSource, List(Of USER))
        For Each user In userList
            Dim relationship As New USER_USER_ROLE_GAME_FIRM_RELATIONSHIP
            relationship.IS_DELETED = False
            relationship.USER = user
            relationship.GAME = selectedGame
            getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP.Add(relationship)
        Next

        getDatabaseEntity.SaveChanges()
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        Dim addEditPeriodUserControl As New AddEditGamePeriod(New PERIOD, 1)
        periodAddEditContainer.Content = addEditPeriodUserControl
    End Sub

    Private Sub addImageButton_Click(sender As Object, e As RoutedEventArgs) Handles addImageButton.Click
        Dim imageManager As New ImageManagerMainInterface
        imageManager.afterSelectFunctionDelegate = AddressOf updateSelectedImage
        PopupDialogFunctions.open_dialog(imageManager)
    End Sub

    Private Sub updateSelectedImage(selectedManagerID As Integer)
        Dim viewer = New ImageViewer(selectedManagerID)
        viewer.hideButtonPanel()
        imageContainer.Content = viewer
        PopupDialogFunctions.close_dialog()
    End Sub

    Private Sub addUserButton_Click(sender As Object, e As RoutedEventArgs) Handles addUserButton.Click
        addUserLayoutGroup.Visibility = Visibility.Visible
    End Sub

    Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
        loadSelectedPeriod()
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If periodGridControl.SelectedItem IsNot Nothing Then
            Dim selectedPeriod As PERIOD = DirectCast(periodGridControl.SelectedItem, PERIOD)
            selectedPeriod.IS_DELETED = True
            DirectCast(periodGridControl.ItemsSource, List(Of PERIOD)).Remove(selectedPeriod)
            updatePeriodGridLayout()
        End If
    End Sub

    Private Sub updatePeriodGridLayout()
        periodGridControl.RefreshData()
        periodGridControl.UpdateLayout()
        periodGridView.UpdateLayout()
        periodGridView.BestFitColumns()
    End Sub

    Private Sub updateUserGridLayout()
        userGridControl.RefreshData()
        userGridControl.UpdateLayout()
        userGridView.UpdateLayout()
        userGridView.BestFitColumns()
    End Sub

    Private Sub deleteUserButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteUserButton.Click
        If userGridControl.SelectedItem IsNot Nothing Then
            Dim deleteUser As USER = DirectCast(userGridControl.SelectedItem, USER)
            DirectCast(userGridControl.ItemsSource, List(Of USER)).Remove(deleteUser)
            Dim relationship = deleteUser.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP.ToList.Find(Function(x)
                                                                                                If x.GAME_LINK_ID = selectedGame.ID Then
                                                                                                    Return True
                                                                                                Else
                                                                                                    Return False
                                                                                                End If
                                                                                            End Function)
            If relationship IsNot Nothing Then
                relationship.IS_DELETED = True
                getDatabaseEntity.SaveChanges()
            End If
            availableUserList.Add(deleteUser)
            userSelectCombobox.RefreshData()
            userSelectCombobox.UpdateLayout()
            updateUserGridLayout()
        End If
    End Sub

    Private Sub saveButton_Click(sender As Object, e As RoutedEventArgs) Handles saveButton.Click
        If periodAddEditContainer.Content IsNot Nothing Then
            Dim addEditControl = DirectCast(periodAddEditContainer.Content, AddEditGamePeriod)
            If addEditControl.mode = 0 Then
                Dim period As PERIOD = addEditControl.pullPeriodFromUI
            Else
                Dim period As PERIOD = addEditControl.pullPeriodFromUI()
                getDatabaseEntity.PERIODs.Add(period)
                DirectCast(periodGridControl.ItemsSource, List(Of PERIOD)).Add(period)
            End If
            updatePeriodGridLayout()
            loadSelectedPeriod()
        End If
    End Sub

    Private Sub periodGridView_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles periodGridView.MouseDown
        loadSelectedPeriod()
    End Sub

    Private Sub loadSelectedPeriod()
        If periodGridControl.SelectedItem IsNot Nothing Then
            Dim selectedPeriod As PERIOD = DirectCast(periodGridControl.SelectedItem, PERIOD)
            Dim addEditPeriodUserControl As New AddEditGamePeriod(selectedPeriod, 0)
            periodAddEditContainer.Content = addEditPeriodUserControl
        Else
            periodAddEditContainer.Content = Nothing
        End If
    End Sub

    Private Sub cancelAddUserButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelAddUserButton.Click
        addUserLayoutGroup.Visibility = Visibility.Collapsed
    End Sub

    Private Sub userSelectCombobox_PopupClosed(sender As Object, e As ClosePopupEventArgs) Handles userSelectCombobox.PopupClosed
        'If firmAssignComboBox.EditValue Is Nothing OrElse firmAssignComboBox.EditValue.ToString.Trim.Length = 0 Then
        '    MessageBox.Show("Please select a firm to asign the user to before selecting the user")
        '    firmAssignComboBox.Focus()
        'Else
        '    Dim selectedUsers As New List(Of USER)
        '    Dim assignedFirmId = Integer.Parse(firmAssignComboBox.EditValue.ToString)

        '    For Each item In userSelectCombobox.SelectedItem
        '        Dim user As USER = DirectCast(item, USER)
        '        Dim relationship As New USER_USER_ROLE_GAME_FIRM_RELATIONSHIP
        '        relationship.IS_DELETED = False
        '        relationship.USER_LINK_ID = user.ID
        '        relationship.GAME = selectedGame
        '        relationship.FIRM_LINK_ID = assignedFirmId
        '        getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP.Add(relationship)
        '        getDatabaseEntity.SaveChanges()
        '        selectedUsers.Add(user)
        '    Next

        '    Dim currentItemSource = DirectCast(userGridControl.ItemsSource, List(Of USER))
        '    For Each user In selectedUsers
        '        availableUserList.Remove(user)
        '        currentItemSource.Add(user)
        '    Next
        '    userSelectCombobox.UnselectAllItems()
        '    userSelectCombobox.RefreshData()
        '    userSelectCombobox.UpdateLayout()
        '    updateUserGridLayout()
        'End If
    End Sub

    Private Sub AddEditGameUserControl_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles Me.RequestBringIntoView
        e.Handled = True
    End Sub

    Private Sub userSelectCombobox_EditValueChanged(sender As Object, e As EditValueChangedEventArgs)
        If userSelectCombobox.EditValue Is Nothing Then
            Return
        End If
        If firmAssignComboBox.EditValue Is Nothing OrElse firmAssignComboBox.EditValue.ToString.Trim.Length = 0 Then
            MessageBox.Show("Please select a firm to asign the user to before selecting the user")
            firmAssignComboBox.Focus()
        Else
            Dim selectedUsers As New List(Of USER)
            Dim assignedFirmId = Integer.Parse(firmAssignComboBox.EditValue.ToString)

            For Each item In userSelectCombobox.EditValue
                Dim user As USER = DirectCast(item, USER)
                Dim relationship As New USER_USER_ROLE_GAME_FIRM_RELATIONSHIP
                relationship.IS_DELETED = False
                relationship.USER_LINK_ID = user.ID
                relationship.GAME = selectedGame
                relationship.FIRM_LINK_ID = assignedFirmId
                getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP.Add(relationship)
                getDatabaseEntity.SaveChanges()
                selectedUsers.Add(user)
            Next

            Dim currentItemSource = DirectCast(userGridControl.ItemsSource, List(Of USER))
            For Each user In selectedUsers
                availableUserList.Remove(user)
                currentItemSource.Add(user)
            Next
            userSelectCombobox.UnselectAllItems()
            userSelectCombobox.RefreshData()
            userSelectCombobox.UpdateLayout()
            updateUserGridLayout()
        End If
    End Sub

    Private Sub firmAssignComboBox_EditValueChanged(sender As Object, e As EditValueChangedEventArgs)
        userSelectCombobox.EditValue = Nothing
    End Sub
End Class
