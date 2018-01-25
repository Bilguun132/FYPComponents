Imports System.Windows
Imports GameAdministratorCenter.Contracts

Public Class EditMarketInfoUserControl
    Private marketInfoEntity As MARKET_INFO

    Public Sub New(Optional passedInEntity As MARKET_INFO = Nothing)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If passedInEntity IsNot Nothing Then
            marketInfoEntity = passedInEntity
        Else
            marketInfoEntity = (From query In getDatabaseEntity.MARKET_INFO Where query.ID = 1).FirstOrDefault
        End If

        Me.DataContext = marketInfoEntity
    End Sub

    Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
        UndoingChangesDbEntityLevel(getDatabaseEntity, marketInfoEntity)
    End Sub

    Private Sub saveButton_Click(sender As Object, e As RoutedEventArgs) Handles saveButton.Click
        If marketInfoEntity.ID = 0 Then
            getDatabaseEntity.MARKET_INFO.Add(marketInfoEntity)
        End If

        getDatabaseEntity.SaveChanges()
        MessageBox.Show("Save Success")
    End Sub
End Class
