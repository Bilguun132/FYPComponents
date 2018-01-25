Imports GameAdministratorCenter.Contracts

Public Class BalanceTransactionModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer?
    Public Property businessAspect As String
    Public Property balanceSheetLinkId As Integer?
    Public Property transactionAmount As Decimal?
    Public Property transactionName As String
    Public Property targetType As String
    Public Property transactionJson As String
    Public Property dateTimeInserted As Date
    Public Property revenueAndCostLinkId As Integer?

End Class
