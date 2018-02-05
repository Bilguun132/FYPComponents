Imports GameAdministratorCenter.Contracts

Public Class MarketingDecisionModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer?
    Public Property price As Decimal?
    Public Property setDateInGame As Decimal?
    Public Property firmLinkId As Integer?
    Public Property userLinkId As Integer?
    Public Property periodLinkId As Integer?
    Public Property marketShareQtyChange As Decimal?
End Class
