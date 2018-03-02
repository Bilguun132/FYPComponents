Imports System.Windows
Imports GameAdministratorCenter.Contracts

Public Class FirmListUserControl
    Private gameId As Integer?
    Private firmList As New List(Of GAME_FIRM)
    Private Delegate Sub confirmButtonClick()
    Private confirmButtonClickDelegate As confirmButtonClick = Nothing

    Public Sub New(gameLinkedId As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        gameId = gameLinkedId
        addTemplateButton.Visibility = Visibility.Visible
        loadPage()
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        loadPage()
    End Sub

    Private Sub loadPage()
        If gameId IsNot Nothing Then
            firmList = (From query In getDatabaseEntity.GAME_FIRM Where query.GAME_LINK_ID = gameId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
        Else
            firmList = (From query In getDatabaseEntity.GAME_FIRM Where query.GAME_LINK_ID Is Nothing AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
        End If

        firmGrid.ItemsSource = firmList
        firmGridView.BestFitColumns()
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        passwordInputPopup.PlacementTarget = sender
        confirmButtonClickDelegate = AddressOf addFirm
        If PublicVariables.hasAccess Then
            If confirmButtonClickDelegate IsNot Nothing Then
                confirmButtonClickDelegate()
            End If
            Return
        End If
        passwordTextEdit.Text = ""
        passwordInputPopup.IsOpen = True
    End Sub

    Private Sub addFirm()
        Dim addEditFirm As AddEditFirm
        If gameId IsNot Nothing Then
            addEditFirm = New AddEditFirm(gameId)
        Else
            addEditFirm = New AddEditFirm()
        End If
        addEditFirm.backButtonClickedDelegate = AddressOf toggleView
        addEditFirm.saveButtonClickedDelegate = AddressOf updateGridData
        addEditPageContainer.Content = addEditFirm
        toggleView()
    End Sub

    Private Sub updateGridData(firmEntity As GAME_FIRM)
        If Not firmList.Contains(firmEntity) Then
            firmList.Add(firmEntity)
        End If

        updateGridLayout()
        toggleView()
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        If firmGrid.SelectedItem IsNot Nothing Then
            Dim selectedFirm As GAME_FIRM = DirectCast(firmGrid.SelectedItem, GAME_FIRM)
            Dim addEditFirm As AddEditFirm = New AddEditFirm(selectedFirm)
            addEditFirm.backButtonClickedDelegate = AddressOf toggleView
            addEditFirm.saveButtonClickedDelegate = AddressOf updateGridData
            addEditPageContainer.Content = addEditFirm
            toggleView()
        End If
    End Sub

    Private Sub toggleView()
        If listContainer.Visibility = Visibility.Visible Then
            listContainer.Visibility = Visibility.Collapsed
            addEditPageContainer.Visibility = Visibility.Visible
        Else
            listContainer.Visibility = Visibility.Visible
            addEditPageContainer.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        passwordInputPopup.PlacementTarget = sender
        confirmButtonClickDelegate = AddressOf deleteFirm
        passwordTextEdit.Text = ""
        passwordInputPopup.IsOpen = True
    End Sub

    Private Sub deleteFirm()
        If firmGrid.SelectedItem IsNot Nothing Then
            Dim selectedFirm As GAME_FIRM = DirectCast(firmGrid.SelectedItem, GAME_FIRM)
            selectedFirm.IS_DELETED = True
            getDatabaseEntity.SaveChanges()
            firmList.Remove(selectedFirm)
            updateGridLayout()
        End If
    End Sub

    Private Sub updateGridLayout()
        firmGrid.RefreshData()
        firmGrid.UpdateLayout()
        firmGridView.UpdateLayout()
        firmGridView.BestFitColumns()
    End Sub

    Private Sub addTemplateButton_Click(sender As Object, e As RoutedEventArgs) Handles addTemplateButton.Click
        templateContainer.Visibility = Visibility.Visible
        Dim templateList = (From query In getDatabaseEntity.GAME_FIRM Where query.GAME_LINK_ID Is Nothing AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
        templateFirmCombobox.ItemsSource = Nothing
        templateFirmCombobox.UnselectAllItems()

        templateFirmCombobox.ItemsSource = templateList
    End Sub

    Private Sub templateFirmCombobox_PopupClosed(sender As Object, e As DevExpress.Xpf.Editors.ClosePopupEventArgs)
        addTemplateFirms()
    End Sub

    Private Sub addTemplateFirms()
        If templateFirmCombobox.SelectedItems IsNot Nothing Then
            For Each item In templateFirmCombobox.SelectedItems
                Dim templateFirmEntity As GAME_FIRM = DirectCast(item, GAME_FIRM)
                addFirmWithTemplate(templateFirmEntity, gameId)
            Next
            getDatabaseEntity.SaveChanges()
            loadPage()
        End If

        templateContainer.Visibility = Visibility.Collapsed
    End Sub

    Private Sub addFirmWithTemplate(templateFirmEntity As GAME_FIRM, gameId As Integer?)
        Dim newFirm As New GAME_FIRM
        newFirm.IS_DELETED = False
        newFirm.GAME_LINK_ID = gameId
        newFirm.FIRM_NAME = templateFirmEntity.FIRM_NAME
        newFirm.DESCRIPTION = templateFirmEntity.DESCRIPTION
        newFirm.MAX_NUMBER_OF_PLAYER = templateFirmEntity.MAX_NUMBER_OF_PLAYER
        newFirm.PRODUCTION_QUALITY = templateFirmEntity.PRODUCTION_QUALITY
        newFirm.PRODUCTION_PRICE = templateFirmEntity.PRODUCTION_PRICE
        newFirm.PRODUCTION_COST = templateFirmEntity.PRODUCTION_COST
        newFirm.MARKET_INFO_LINK_ID = 3
        getDatabaseEntity.GAME_FIRM.Add(newFirm)

        If templateFirmEntity.BALANCE_SHEET IsNot Nothing Then
            Dim newBalanceSheet As New BALANCE_SHEET
            newBalanceSheet.IS_DELETED = False
            newFirm.BALANCE_SHEET = newBalanceSheet
            newBalanceSheet.INITIAL_ASSET = templateFirmEntity.BALANCE_SHEET.INITIAL_ASSET
            newBalanceSheet.INITIAL_LIABILITY = templateFirmEntity.BALANCE_SHEET.INITIAL_LIABILITY
            newBalanceSheet.INITIAL_EQUITY = templateFirmEntity.BALANCE_SHEET.INITIAL_EQUITY
            newBalanceSheet.TOTAL_ASSET = templateFirmEntity.BALANCE_SHEET.TOTAL_ASSET
            newBalanceSheet.TOTAL_EQUITY = templateFirmEntity.BALANCE_SHEET.TOTAL_EQUITY
            newBalanceSheet.TOTAL_LIABILITY = templateFirmEntity.BALANCE_SHEET.TOTAL_LIABILITY
            getDatabaseEntity.BALANCE_SHEET.Add(newBalanceSheet)
        End If

        If templateFirmEntity.REVENUE_AND_COST IsNot Nothing AndAlso templateFirmEntity.REVENUE_AND_COST.Count > 0 Then
            For Each revenueAndCostEntity In templateFirmEntity.REVENUE_AND_COST
                Dim newSource As New REVENUE_AND_COST
                newSource.IS_DELETED = False
                newSource.NAME = revenueAndCostEntity.NAME
                newSource.GAME_FIRM = newFirm
                newSource.FLOW_TYPE = revenueAndCostEntity.FLOW_TYPE
                newSource.PAYMENT_TYPE = revenueAndCostEntity.PAYMENT_TYPE
                newSource.DESCRIPTION = revenueAndCostEntity.DESCRIPTION
                newSource.PAYMENT_AMOUNT = revenueAndCostEntity.PAYMENT_AMOUNT
                newSource.FIRST_PAYMENT_DATE = revenueAndCostEntity.FIRST_PAYMENT_DATE
                newSource.RECURSIVE_DURATION = revenueAndCostEntity.RECURSIVE_DURATION
                newSource.PERIOD_NUMBER = revenueAndCostEntity.PERIOD_NUMBER
                newSource.TARGET_TYPE = revenueAndCostEntity.TARGET_TYPE
                getDatabaseEntity.REVENUE_AND_COST.Add(newSource)
            Next
        End If
    End Sub

    Private Sub confirmButton_Click(sender As Object, e As RoutedEventArgs) Handles confirmButton.Click
        If passwordTextEdit.Text.Equals(PublicVariables.adminPassword) Then
            If confirmButtonClickDelegate IsNot Nothing Then
                confirmButtonClickDelegate()
            End If
        Else
            MessageBox.Show("Incorrect password")
        End If
        passwordInputPopup.IsOpen = False
    End Sub


End Class
