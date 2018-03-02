Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class FirmModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer
    Public Property firmName As String
    Public Property gameLinkId As String
    Public Property firmDescription As String
    Public Property maxNumberOfPlayers As String
    Public Property productionPrice As String
    Public Property productionQuality As Integer?
    Public Property marketShare As Double?
    Public Property marketSharePercentage As Double?
    Public Property stLoanLimit As Integer?
    Public Property ltLoanLimit As Integer?
End Class
