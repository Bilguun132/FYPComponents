Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure

Public Module PublicFunctions

#Region "Delegates"
    Public Delegate Sub ShowThisUserControl(ByVal userControl As Object)
    Public ShowThisUserControlCallback As ShowThisUserControl = Nothing
#End Region

    Public Sub UndoingChangesDbEntityLevel(ByVal context As DbContext, ByVal entity As Object)
        Dim entry As DbEntityEntry = context.Entry(entity)
        Select Case entry.State
            Case EntityState.Modified
                entry.State = EntityState.Unchanged
            Case EntityState.Added
                entry.State = EntityState.Detached
            Case EntityState.Deleted
                entry.Reload()
            Case Else
        End Select
    End Sub

    Public Sub updateProductionRevenue(periodId As Integer)
        Dim databaseEntities = getDatabaseEntity()

        Dim period As PERIOD = (From query In getDatabaseEntity.PERIODs Where query.ID = periodId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If period IsNot Nothing AndAlso period.GAME IsNot Nothing AndAlso period.GAME.GAME_FIRM IsNot Nothing Then
            For Each firm In period.GAME.GAME_FIRM
                Dim totalProductionQuantity As Decimal = 0
                Dim productionDecisionList As List(Of PRODUCTION_DECISIONS) = (From query In databaseEntities.PRODUCTION_DECISIONS Where query.FIRM_LINK_ID = firm.ID AndAlso query.PERIOD_LINK_ID = periodId AndAlso
                                                                                                                                   (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
                If productionDecisionList IsNot Nothing Then
                    For Each productionDecision In productionDecisionList
                        If productionDecision.QUANTITY IsNot Nothing Then
                            totalProductionQuantity += productionDecision.QUANTITY
                        End If
                    Next
                End If

                If firm.MARKET_SHARE_QTY IsNot Nothing AndAlso firm.PRODUCTION_PRICE IsNot Nothing Then
                    Dim amountSold = Math.Max(Integer.Parse(firm.MARKET_SHARE_QTY), totalProductionQuantity)

                    Dim revenueAndCost As New REVENUE_AND_COST
                    revenueAndCost.FLOW_TYPE = CashflowType.revenue
                    revenueAndCost.PAYMENT_TYPE = PaymentType.oneTime
                    revenueAndCost.RECURSIVE_DURATION = 0
                    revenueAndCost.PAYMENT_AMOUNT = amountSold * firm.PRODUCTION_PRICE
                    revenueAndCost.NAME = "SaleRevenue"
                    revenueAndCost.GAME_FIRM = firm
                    revenueAndCost.PERIOD = period
                    revenueAndCost.IS_DELETED = False
                    revenueAndCost.TARGET_TYPE = PaymentTargetType.equity
                    revenueAndCost.FIRST_PAYMENT_DATE = period.SIMULATION_DURATION
                    revenueAndCost.BUSINESS_ASPECT = BusinessAspect.production
                    databaseEntities.REVENUE_AND_COST.Add(revenueAndCost)

                    '  subscribeRevenueAndCost(revenueAndCost.ID)
                End If
            Next

        End If
    End Sub

    Public Sub updateMarketInfo(ByRef passedInMarketInfo As MARKET_INFO, numberOfYearPassed As Decimal)
        If passedInMarketInfo.MARKET_SIZE IsNot Nothing AndAlso passedInMarketInfo.GROWTH_RATE IsNot Nothing Then
            Dim absoluteGrowth As Decimal = passedInMarketInfo.MARKET_SIZE * passedInMarketInfo.GROWTH_RATE * numberOfYearPassed / 100
            passedInMarketInfo.MARKET_SIZE += absoluteGrowth
            getDatabaseEntity.SaveChanges()
        End If
    End Sub

    Public Function findCurrentActivePeriod(firmId As Integer, databaseEntities As FYP_DATABASEEntities) As PERIOD
        Dim firm As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If firm IsNot Nothing Then
            Dim periodList As List(Of PERIOD) = (From query In databaseEntities.PERIODs Where query.GAME_LINK_ID = firm.GAME_LINK_ID AndAlso query.PERIOD_NUMBER IsNot Nothing AndAlso
                                                                                         (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
            If periodList IsNot Nothing AndAlso periodList.Count > 0 Then
                periodList.Sort(Function(x, y)
                                    If x.PERIOD_NUMBER < y.PERIOD_NUMBER Then
                                        Return -1
                                    ElseIf x.PERIOD_NUMBER > y.PERIOD_NUMBER Then
                                        Return 1
                                    Else
                                        Return 0
                                    End If
                                End Function)
                Dim currentPeriod = periodList.Find(Function(x)
                                                        If x.START_TIME IsNot Nothing AndAlso x.END_TIME Is Nothing Then
                                                            Return True
                                                        Else
                                                            Return False
                                                        End If
                                                    End Function)
                Return currentPeriod
            End If
        End If

        Return Nothing
    End Function

    'Public Sub subscribeRevenueAndCost(revenueAndCostId As Integer)
    '    Dim runnerClass As New ServiceThreadClass
    '    runnerClass.idType = IdType.revenueAndCost
    '    runnerClass.id = revenueAndCostId
    '    revenueAndCostThreadList.Add(runnerClass)
    '    runnerClass.runRevenueAndCostService()
    'End Sub

    'Public Sub unsubsribeRevenueAndcost(iD As Integer)
    '    Dim revenueAndCostService As ServiceThreadClass = revenueAndCostThreadList.Find(Function(x)
    '                                                                                        If x.idType = IdType.revenueAndCost AndAlso x.id = iD Then
    '                                                                                            Return True
    '                                                                                        Else
    '                                                                                            Return False
    '                                                                                        End If
    '                                                                                    End Function)
    '    If revenueAndCostService IsNot Nothing Then
    '        revenueAndCostService.stopRevenueAndCostService()
    '    End If
    'End Sub
End Module
