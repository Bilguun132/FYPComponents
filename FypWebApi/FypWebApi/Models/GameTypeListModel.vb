Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class GameTypeListModel
    Inherits BaseModel

    Public Property gameTypeList As List(Of GameTypeModel)
    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub
End Class
