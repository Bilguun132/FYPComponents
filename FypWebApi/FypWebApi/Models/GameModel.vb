Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class GameModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer
    Public Property gameName As String
    Public Property gameType As String
    Public Property gameDescription As String
    Public Property gameRecommendedPlayers As String
    Public Property gameImageString As String
    Public Property gameTypeId As Integer?
    Public Property gamePassword As String
    Public Property joinedFirmId As Integer
End Class
