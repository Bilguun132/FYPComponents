Imports GameAdministratorCenter.Contracts

Public Class BaseModel
    Public Property responseCode As Enums
    Public Property responseMessage As String

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        responseCode = passedInCode
        responseMessage = passedInMessage
    End Sub
End Class
