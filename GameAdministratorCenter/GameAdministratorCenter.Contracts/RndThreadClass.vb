Imports System.Threading
Imports GameAdministratorCenter.Contracts

Public Class ServiceThreadClass
    Public Property id As Integer?
    Public Property idType As IdType
    Public Property runningThread As Thread

    Public Sub runRevenueAndCostService()
        runningThread = New Thread(AddressOf runService)
        runningThread.Start()
    End Sub

    Private Sub runService()
        Dim databaseEntities = New FYP_DATABASEEntities
        Dim revenueAndCost As REVENUE_AND_COST = (From query In databaseEntities.REVENUE_AND_COST Where query.ID = id AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If revenueAndCost IsNot Nothing AndAlso revenueAndCost.FIRM_LINK_ID IsNot Nothing Then
            Dim currentPeriod = findCurrentActivePeriod(revenueAndCost.FIRM_LINK_ID, databaseEntities)
            If currentPeriod IsNot Nothing AndAlso currentPeriod.ID = revenueAndCost.PERIOD_LINK_ID Then
                Dim timeToNextIncrement As Decimal? = findTimeToNextIncrement(currentPeriod.ID, revenueAndCost.ID)
                While timeToNextIncrement IsNot Nothing
                    Thread.Sleep(timeToNextIncrement * 1000)
                    performIncrement(currentPeriod.ID, revenueAndCost.ID)
                    timeToNextIncrement = findTimeToNextIncrement(currentPeriod.ID, revenueAndCost.ID)
                End While
            End If
        End If
    End Sub

    Private Sub performIncrement(currentPeriodId As Integer, revenueAndCostId As Integer)
        Dim databaseEntities = New FYP_DATABASEEntities
        Dim revenueAndCost As REVENUE_AND_COST = (From query In databaseEntities.REVENUE_AND_COST Where query.ID = id AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If revenueAndCost IsNot Nothing AndAlso revenueAndCost.FIRM_LINK_ID IsNot Nothing Then
            Dim currentPeriod = findCurrentActivePeriod(revenueAndCost.FIRM_LINK_ID, databaseEntities)
            Dim balanceSheet = revenueAndCost.GAME_FIRM.BALANCE_SHEET
            If currentPeriod IsNot Nothing AndAlso currentPeriod.ID = revenueAndCost.PERIOD_LINK_ID Then
                Dim balanceTransaction As New BALANCE_SHEET_TRANSACTION
                balanceTransaction.BALANCE_SHEET = balanceSheet
                balanceTransaction.DATE_TIME_INSERTED = Date.Now
                balanceTransaction.REVENUE_AND_COST = revenueAndCost
                If revenueAndCost.FLOW_TYPE = CashflowType.cost Then
                    balanceTransaction.TRANSACTION_AMOUNT = -revenueAndCost.PAYMENT_AMOUNT
                Else
                    balanceTransaction.TRANSACTION_AMOUNT = revenueAndCost.PAYMENT_AMOUNT
                End If
                balanceTransaction.TRANSACTION_TYPE = revenueAndCost.TARGET_TYPE
                balanceTransaction.TRANSACTION_NAME = revenueAndCost.NAME
                balanceTransaction.TRANSACTION_STATUS = revenueAndCost.BUSINESS_ASPECT
                databaseEntities.BALANCE_SHEET_TRANSACTION.Add(balanceTransaction)

                Select Case revenueAndCost.TARGET_TYPE
                    Case PaymentTargetType.asset
                        balanceSheet.TOTAL_ASSET += balanceTransaction.TRANSACTION_AMOUNT
                    Case PaymentTargetType.equity
                        balanceSheet.TOTAL_EQUITY += balanceTransaction.TRANSACTION_AMOUNT
                    Case PaymentTargetType.liability
                        balanceSheet.TOTAL_LIABILITY += balanceTransaction.TRANSACTION_AMOUNT
                End Select

                databaseEntities.SaveChanges()
                computeAndReturnRevenueAndCostResult(revenueAndCostId, currentPeriodId)
            End If
        End If
    End Sub

    Private Sub computeAndReturnRevenueAndCostResult(revenueAndCostId As Integer, currentPeriodId As Integer)
        Dim databaseEntities = New FYP_DATABASEEntities
        Dim revenueAndCost As REVENUE_AND_COST = (From query In databaseEntities.REVENUE_AND_COST Where query.ID = id AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If revenueAndCost IsNot Nothing Then
            Select Case revenueAndCost.NAME
                Case "RndMonthlyCost"
                    generateRndResult(databaseEntities, revenueAndCost, currentPeriodId)
            End Select
        End If
    End Sub

    Private Sub generateRndResult(databaseEntities As FYP_DATABASEEntities, revenueAndCost As REVENUE_AND_COST, currentPeriodId As Integer)
        Dim rndResult As New RESULT
        rndResult.ACTION_FIRM_LINK_ID = revenueAndCost.FIRM_LINK_ID
        rndResult.TARGET_FIRM_ID = revenueAndCost.FIRM_LINK_ID
        rndResult.IS_DELETED = False
        rndResult.PERIOD_LINK_ID = currentPeriodId
        rndResult.RND_DECISION_LINK_ID = revenueAndCost.RND_DECISIONS.ElementAt(0).ID

        Dim firmEntity = (From query In databaseEntities.GAME_FIRM Where query.ID = revenueAndCost.FIRM_LINK_ID).FirstOrDefault

        Dim randomNumber = New Random().Next(0, 100)
        If randomNumber <= 50 Then
            Dim qualityMax = 100
            Dim qualityToMax = qualityMax - firmEntity.PRODUCTION_QUALITY
            Dim qualityChange = revenueAndCost.PAYMENT_AMOUNT / 1000 * 1 * qualityToMax
            rndResult.QUALITY_CHANGE = qualityChange
            firmEntity.PRODUCTION_QUALITY += qualityChange
        Else
            Dim productionCostMin = 50
            Dim reductionToMin = firmEntity.PRODUCTION_COST - productionCostMin
            rndResult.PRODUCTION_COST_CHANGE = -revenueAndCost.PAYMENT_AMOUNT / 1000 * 1 * reductionToMin
            firmEntity.PRODUCTION_COST += rndResult.PRODUCTION_COST_CHANGE
        End If
        databaseEntities.RESULTs.Add(rndResult)

        databaseEntities.SaveChanges()
    End Sub

    Private Function findTimeToNextIncrement(currentPeriodId As Integer, revenueAndCostId As Integer) As Decimal?
        Dim databaseEntities = New FYP_DATABASEEntities
        Dim revenueAndCost As REVENUE_AND_COST = (From query In databaseEntities.REVENUE_AND_COST Where query.ID = id AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If revenueAndCost IsNot Nothing AndAlso revenueAndCost.FIRM_LINK_ID IsNot Nothing Then
            Dim currentPeriod = findCurrentActivePeriod(revenueAndCost.FIRM_LINK_ID, databaseEntities)
            If currentPeriod IsNot Nothing AndAlso currentPeriod.ID = revenueAndCost.PERIOD_LINK_ID AndAlso
                currentPeriod.EXPECTED_END IsNot Nothing AndAlso currentPeriod.EXPECTED_START IsNot Nothing AndAlso
                currentPeriod.SIMULATION_DURATION IsNot Nothing Then

                Dim numberOfSecondForOneDay = (Date.Parse(currentPeriod.EXPECTED_END) - Date.Parse(currentPeriod.EXPECTED_START)).TotalSeconds / currentPeriod.SIMULATION_DURATION
                Dim firstPayment As Date = (currentPeriod.START_TIME + TimeSpan.FromSeconds(revenueAndCost.FIRST_PAYMENT_DATE * numberOfSecondForOneDay))
                If firstPayment > Date.Now Then
                    Return (firstPayment - Date.Now).TotalSeconds
                Else
                    Dim incrementSpan As Decimal
                    Select Case revenueAndCost.PAYMENT_TYPE
                        Case PaymentType.oneTime
                            Return 0
                        Case PaymentType.weeklyRecursive
                            incrementSpan = 7
                        Case PaymentType.monthlyRecursive
                            incrementSpan = 30
                        Case PaymentType.yearlyRecursive
                            incrementSpan = 360
                    End Select

                    Dim counter = 1
                    While revenueAndCost.RECURSIVE_DURATION = 0 OrElse counter < revenueAndCost.RECURSIVE_DURATION
                        Dim paymentTime As Date = firstPayment + TimeSpan.FromSeconds(counter * incrementSpan * numberOfSecondForOneDay)
                        If paymentTime > Date.Now Then
                            Return (paymentTime - Date.Now).TotalSeconds
                        End If
                        counter += 1
                    End While
                End If
            End If
        End If

        Return Nothing
    End Function

    Public Sub stopRevenueAndCostService()
        If runningThread IsNot Nothing Then
            runningThread.Abort()
            runningThread = Nothing
        End If
    End Sub
End Class
