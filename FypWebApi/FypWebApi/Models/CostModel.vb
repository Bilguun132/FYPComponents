Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class CostModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property id As Integer?
    Public Property cashflowType As CashflowType?
    Public Property paymentType As PaymentType?
    Public Property paymentTargetType As PaymentTargetType?
    Public Property businessAspect As BusinessAspect?
    Public Property paymentAmount As Double?
    Public Property name As String
    Public Property description As String
    Public Property numberOfPayment As Integer?
    Public Property firstPaymentDate As Integer?
    Public Property periodNumber As Integer?

End Class

Public Class CostListModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property costingList As New List(Of CostModel)
End Class
