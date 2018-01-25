Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class GamePeriodModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer
    Public Property periodName As String
    Public Property periodNumber As String
    Public Property status As GamePeriodStatus
    Public Property startTime As DateTime?
    Public Property endTime As DateTime?
    Public Property expectedStart As DateTime
    Public Property expectedEnd As DateTime
    Public Property gameLinkId As Integer
    Public Property previousPeriodId As Integer?
End Class
