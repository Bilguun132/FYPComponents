Imports GameAdministratorCenter.Contracts

Public Class FinanceDecisionModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer?
    Public Property firmLinkId As Integer?
    Public Property periodLinkId As Integer?
    Public Property setDateInGame As Decimal?
    Public Property userLinkId As Integer?
    Public Property revenueAndCostLinkId As Integer?
    Public Property stLoan As Decimal?
    Public Property ltLoan As Decimal?
End Class
