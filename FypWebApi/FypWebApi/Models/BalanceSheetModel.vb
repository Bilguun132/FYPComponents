Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class BalanceSheetModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer
    Public Property totalAsset As String
    Public Property totalLiability As String
    Public Property totalEquity As String
End Class
