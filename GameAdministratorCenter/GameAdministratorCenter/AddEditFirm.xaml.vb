Imports System.Windows
Imports GameAdministratorCenter.Contracts

Public Class AddEditFirm
    Private gameLinkedId As Integer?
    Private firmEntity As GAME_FIRM
    Private revenueCostList As New List(Of REVENUE_AND_COST)
    Public Delegate Sub backButtonClicked()
    Public backButtonClickedDelegate As backButtonClicked = Nothing
    Public Delegate Sub saveButtonClicked(firmEntity As GAME_FIRM)
    Public saveButtonClickedDelegate As saveButtonClicked = Nothing

    Public Sub New(passedInGameId As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        gameLinkedId = passedInGameId
        firmEntity = New GAME_FIRM
        firmEntity.GAME_LINK_ID = gameLinkedId
        firmEntity.IS_DELETED = False
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        firmEntity = New GAME_FIRM
        firmEntity.IS_DELETED = False
    End Sub


    Public Sub New(gameFirmEntity As GAME_FIRM)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        firmEntity = gameFirmEntity
    End Sub

    Private Sub AddEditFirm_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        loadPage()
    End Sub

    Private Sub loadPage()
        If firmEntity IsNot Nothing Then
            firmNameTextBox.Text = firmEntity.FIRM_NAME
            If firmEntity.MAX_NUMBER_OF_PLAYER IsNot Nothing AndAlso firmEntity.MAX_NUMBER_OF_PLAYER >= 1 Then
                maxPlayerSpinEdit.EditValue = firmEntity.MAX_NUMBER_OF_PLAYER
            End If
            descriptionTextBox.Text = firmEntity.DESCRIPTION

            productionCostSpinEdit.EditValue = firmEntity.PRODUCTION_COST
            productionPriceSpinEdit.EditValue = firmEntity.PRODUCTION_PRICE
            productionQualitySpinEdit.EditValue = firmEntity.PRODUCTION_QUALITY

            If firmEntity.BALANCE_SHEET IsNot Nothing Then
                If firmEntity.BALANCE_SHEET.INITIAL_ASSET IsNot Nothing Then
                    initialAssetSpinEdit.EditValue = firmEntity.BALANCE_SHEET.INITIAL_ASSET
                End If
                If firmEntity.BALANCE_SHEET.INITIAL_LIABILITY IsNot Nothing Then
                    initialLiabilitySpinEdit.EditValue = firmEntity.BALANCE_SHEET.INITIAL_LIABILITY
                End If
                If firmEntity.BALANCE_SHEET.INITIAL_EQUITY IsNot Nothing Then
                    initialEquitySpinEdit.EditValue = firmEntity.BALANCE_SHEET.INITIAL_EQUITY
                End If
            End If

            If firmEntity.REVENUE_AND_COST IsNot Nothing Then
                revenueCostList = firmEntity.REVENUE_AND_COST.ToList.FindAll(Function(x)
                                                                                 If x.IS_DELETED Is Nothing OrElse x.IS_DELETED = False Then
                                                                                     Return True
                                                                                 Else
                                                                                     Return False
                                                                                 End If
                                                                             End Function).ToList
                If revenueCostList Is Nothing Then
                    revenueCostList = New List(Of REVENUE_AND_COST)
                End If
            End If
        End If

        revenueCostGrid.ItemsSource = revenueCostList
        revenueCostGridView.BestFitColumns()
    End Sub

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        AddEditRevenueCostGroup.DataContext = New REVENUE_AND_COST
        AddEditRevenueCostGroup.Visibility = Visibility.Visible
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        Dim selectedItem = DirectCast(revenueCostGrid.SelectedItem, REVENUE_AND_COST)
        AddEditRevenueCostGroup.DataContext = selectedItem
        AddEditRevenueCostGroup.Visibility = Visibility.Visible
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        Dim selectedItem = DirectCast(revenueCostGrid.SelectedItem, REVENUE_AND_COST)
        selectedItem.IS_DELETED = True
        revenueCostList.Remove(selectedItem)
        updateRevenueCostGridLayout()
    End Sub

    Private Sub updateRevenueCostGridLayout()
        revenueCostGrid.RefreshData()
        revenueCostGrid.UpdateLayout()
        revenueCostGridView.UpdateLayout()
        revenueCostGridView.BestFitColumns()
    End Sub

    Private Sub confirmButton_Click(sender As Object, e As RoutedEventArgs) Handles confirmButton.Click
        If AddEditRevenueCostGroup.DataContext IsNot Nothing Then
            Dim selectedRevenueCost = DirectCast(AddEditRevenueCostGroup.DataContext, REVENUE_AND_COST)
            selectedRevenueCost.GAME_FIRM = firmEntity
            selectedRevenueCost.IS_DELETED = False
            If Not revenueCostList.Contains(selectedRevenueCost) Then
                revenueCostList.Add(selectedRevenueCost)
            End If
            updateRevenueCostGridLayout()
        End If
        AddEditRevenueCostGroup.Visibility = Visibility.Collapsed
    End Sub

    Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
        AddEditRevenueCostGroup.DataContext = Nothing
        AddEditRevenueCostGroup.Visibility = Visibility.Collapsed
    End Sub

    Private Sub backButton_Click(sender As Object, e As RoutedEventArgs) Handles backButton.Click
        If backButtonClickedDelegate IsNot Nothing Then
            backButtonClickedDelegate()
        End If
    End Sub

    Private Sub saveButton_Click(sender As Object, e As RoutedEventArgs) Handles saveButton.Click
        passwordTextEdit.Text = ""
        passwordInputPopup.IsOpen = True
    End Sub

    Private Sub saveFirm()
        firmEntity.FIRM_NAME = firmNameTextBox.Text
        firmEntity.DESCRIPTION = descriptionTextBox.Text

        If productionCostSpinEdit.EditValue IsNot Nothing Then
            firmEntity.PRODUCTION_COST = Decimal.Parse(productionCostSpinEdit.EditValue)
        End If

        If productionPriceSpinEdit.EditValue IsNot Nothing Then
            firmEntity.PRODUCTION_PRICE = Decimal.Parse(productionPriceSpinEdit.EditValue)
        End If

        If productionQualitySpinEdit.EditValue IsNot Nothing Then
            firmEntity.PRODUCTION_QUALITY = Decimal.Parse(productionQualitySpinEdit.EditValue)
        End If

        firmEntity.MARKET_INFO_LINK_ID = 1

        If maxPlayerSpinEdit.EditValue IsNot Nothing Then
            firmEntity.MAX_NUMBER_OF_PLAYER = Integer.Parse(maxPlayerSpinEdit.EditValue)
        End If
        Dim balanceSheet As BALANCE_SHEET
        If firmEntity.BALANCE_SHEET IsNot Nothing Then
            balanceSheet = firmEntity.BALANCE_SHEET
        Else
            balanceSheet = New BALANCE_SHEET
            balanceSheet.IS_DELETED = False
            firmEntity.BALANCE_SHEET = balanceSheet
        End If
        If initialAssetSpinEdit.EditValue IsNot Nothing Then
            balanceSheet.INITIAL_ASSET = Decimal.Parse(initialAssetSpinEdit.EditValue)
            balanceSheet.TOTAL_ASSET = balanceSheet.INITIAL_ASSET
        End If
        If initialLiabilitySpinEdit.EditValue IsNot Nothing Then
            balanceSheet.INITIAL_LIABILITY = Decimal.Parse(initialLiabilitySpinEdit.EditValue)
            balanceSheet.TOTAL_LIABILITY = balanceSheet.INITIAL_LIABILITY
        End If
        If initialEquitySpinEdit.EditValue IsNot Nothing Then
            balanceSheet.INITIAL_EQUITY = Decimal.Parse(initialEquitySpinEdit.EditValue)
            balanceSheet.TOTAL_EQUITY = balanceSheet.INITIAL_EQUITY
        End If

        If firmEntity.ID = 0 Then
            getDatabaseEntity.GAME_FIRM.Add(firmEntity)
        End If

        If balanceSheet.ID = 0 Then
            getDatabaseEntity.BALANCE_SHEET.Add(balanceSheet)
        End If

        For Each source In revenueCostList
            If source.ID = 0 Then
                getDatabaseEntity.REVENUE_AND_COST.Add(source)
            End If
        Next

        getDatabaseEntity.SaveChanges()

        If saveButtonClickedDelegate IsNot Nothing Then
            saveButtonClickedDelegate(firmEntity)
        End If
    End Sub

    Private Sub confirmPasswordButton_Click(sender As Object, e As RoutedEventArgs) Handles confirmPasswordButton.Click
        If passwordTextEdit.Text.Equals(PublicVariables.adminPassword) Then
            saveFirm()
        Else
            MessageBox.Show("Incorrect password")
        End If
        passwordInputPopup.IsOpen = False
    End Sub
End Class
