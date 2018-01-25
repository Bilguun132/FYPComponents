Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class GameListModel
    Inherits BaseModel

    Public Property availableGameList As New List(Of GameModel)
    Public Property joinedGameList As New List(Of GameModel)

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub
End Class
