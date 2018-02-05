Imports FypWebApi
Imports GameAdministratorCenter.Contracts

Public Class IncomeStatementModel
    Inherits BaseModel

    Public Sub New(passedInCode As Enums, passedInMessage As String)
        MyBase.New(passedInCode, passedInMessage)
    End Sub

    Public Property firmId As Integer?
    Public Property firmName As String
    Public Property periodId As Integer?
    Public Property periodNumber As Integer?
    Public Property revenueTransactions As New List(Of BalanceTransactionModel)
    Public Property costTransactions As New List(Of BalanceTransactionModel)
    Public Property marketingDecisions As New List(Of MarketingDecisionModel)
    Public Property productionDecisions As New List(Of ProductionDecisionModel)
    'Public Property rndDecisions As New List(Of RndDecisionModel)

    Public Property totalRevenue As Decimal
    Public Property productionCost As Decimal
    Public Property operatingProfit As Decimal
    Public Property rentSalary As Decimal
    Public Property rndCost As Decimal
    Public Property stInterestExpense As Decimal
    Public Property ltInterestExpense As Decimal
    Public Property totalInterestExpense As Decimal
    Public Property tax As Decimal
    Public Property totalCost As Decimal
    Public Property totalProfit As Decimal
End Class
