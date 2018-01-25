Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class GameTypeModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer
    Public Property gameTypeName As String
    Public Property gameTypeImageString As String
    Public Property gameTypeDescription As String
End Class
