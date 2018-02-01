Imports System.Net.Http
Imports FypWebApi
Imports GameAdministratorCenter.Contracts
Imports Newtonsoft.Json

Public Module GameService
    Dim entities As FYP_DATABASEEntities = getDatabaseEntity()

    Public Function getListOfGameType() As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim gameTypeList As List(Of GAME_TYPE) = (From query In getDatabaseEntity.GAME_TYPE Where query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False).ToList
            Dim returnedModelList As New List(Of GameTypeModel)
            For Each gameType In gameTypeList
                returnedModelList.Add(mapGameType(gameType))
            Next

            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New GameTypeListModel(Enums.success, "")
            wrapperObject.gameTypeList = returnedModelList
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading list of available game types.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getListOfGameByTypeAndUserId(gameTypeId As Integer, userId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim availableGameList As List(Of GAME) = getAvailableGameList(userId, gameTypeId)
            Dim joinedGameList As List(Of GAME) = getJoinedGameList(userId, gameTypeId)
            Dim allGameList As List(Of GAME) = (From query In getDatabaseEntity.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList

            Dim availableGameModelList As New List(Of GameModel)
            Dim joinedGameModelList As New List(Of GameModel)

            For Each game In availableGameList
                availableGameModelList.Add(mapGame(game))
            Next

            For Each game In joinedGameList
                Dim gameModel As GameModel = mapGame(game)

                Dim currentFirm As GAME_FIRM = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                                                          query.USER_LINK_ID = userId AndAlso query.GAME_LINK_ID = game.ID AndAlso query.FIRM_LINK_ID IsNot Nothing Select query.GAME_FIRM).FirstOrDefault
                If currentFirm IsNot Nothing Then
                    gameModel.joinedFirmId = currentFirm.ID
                End If
                joinedGameModelList.Add(gameModel)
            Next

            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New GameListModel(Enums.success, "")
            wrapperObject.availableGameList = availableGameModelList
            wrapperObject.joinedGameList = joinedGameModelList
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading list of available games.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getFirmInfobyFirmId(ByVal firmId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim firm As New FirmModel(Enums.success, "")
            Dim firmEntity As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If firmEntity IsNot Nothing Then
                'ReturnMarketShare(firmEntity)
                firm = mapFirm(firmEntity)
                databaseEntities.SaveChanges()
            End If

            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(firm), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try
        Return returnedMessage
    End Function

    Friend Function getFirmIncomeStatement(firmId As Integer, periodId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim firm As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If firm Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified firm")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim period As PERIOD = (From query In databaseEntities.PERIODs Where query.ID = periodId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If period Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "There is no currently active priod in play for the command to take effect")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim incomeStatement As New IncomeStatementModel(Enums.success, "")
            incomeStatement.firmId = firm.ID
            incomeStatement.firmName = firm.FIRM_NAME
            incomeStatement.periodId = period.ID
            incomeStatement.periodNumber = period.PERIOD_NUMBER

            Dim transactionList As List(Of BALANCE_SHEET_TRANSACTION) = (From query In databaseEntities.BALANCE_SHEET_TRANSACTION Where query.BALANCE_SHEET_LINK_ID = firm.BALANCE_SHEET_LINK_ID AndAlso
                                                                         (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                         query.DATE_TIME_INSERTED >= period.START_TIME AndAlso (period.END_TIME Is Nothing OrElse query.DATE_TIME_INSERTED <= period.END_TIME)).ToList
            For Each transaction In transactionList
                If transaction.TRANSACTION_AMOUNT IsNot Nothing AndAlso transaction.TRANSACTION_AMOUNT < 0 Then
                    incomeStatement.costTransactions.Add(mapTransaction(transaction))
                ElseIf transaction.TRANSACTION_AMOUNT IsNot Nothing AndAlso transaction.TRANSACTION_AMOUNT > 0 Then
                    incomeStatement.revenueTransactions.Add(mapTransaction(transaction))
                End If
            Next

            Dim productionDecisionList As List(Of PRODUCTION_DECISIONS) = (From query In databaseEntities.PRODUCTION_DECISIONS Where query.FIRM_LINK_ID = firmId AndAlso query.PERIOD_LINK_ID = periodId AndAlso
                                                                                                                                   (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
            If productionDecisionList IsNot Nothing Then
                Dim productionDecision = productionDecisionList.Last
                Dim productionQuantity = If(productionDecision.QUANTITY < firm.MARKET_SHARE_QTY, productionDecision.QUANTITY, firm.MARKET_SHARE_QTY)
                incomeStatement.productionDecisions.Add(mapProductionDecision(productionDecision))
                incomeStatement.totalRevenue += productionQuantity * firm.PRODUCTION_PRICE
                incomeStatement.productionCost += productionQuantity * firm.PRODUCTION_COST
                incomeStatement.totalCost += productionQuantity * firm.PRODUCTION_COST
            End If

            Dim marketingDecisionList As List(Of MARKETING_DECISIONS) = (From query In databaseEntities.MARKETING_DECISIONS Where query.FIRM_LINK_ID = firmId AndAlso query.PERIOD_LINK_ID = periodId AndAlso
                                                                                                                                (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
            If marketingDecisionList IsNot Nothing Then
                For Each marketingDecision In marketingDecisionList
                    incomeStatement.marketingDecisions.Add(mapMarketingDecision(marketingDecision))
                Next
            End If

            Dim rndDecision As RND_DECISIONS = (From query In databaseEntities.RND_DECISIONS Where query.FIRM_LINK_ID = firmId AndAlso query.PERIOD_LINK_ID = periodId AndAlso
                                                                                                                                (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList.Last
            Dim endDate As Date = period.EXPECTED_END
            Dim startDate As Date = period.EXPECTED_START
            Dim duration = ((DateDiff(DateInterval.Minute, startDate, endDate) * period.SIMULATION_DURATION) / 30)
            If rndDecision IsNot Nothing Then
                incomeStatement.rndCost = duration * rndDecision.REVENUE_AND_COST.PAYMENT_AMOUNT
            End If
            'For Each revenue In incomeStatement.revenueTransactions
            '    incomeStatement.totalRevenue += revenue.transactionAmount
            'Next

            'For Each cost In incomeStatement.costTransactions
            '    incomeStatement.totalCost += -cost.transactionAmount
            '    If cost.transactionName = "ProductionCost" Then
            '        incomeStatement.productionCost += -cost.transactionAmount
            '    ElseIf cost.transactionName = "RndMonthlyCost" Then
            '        incomeStatement.rndCost += -cost.transactionAmount
            '    Else
            '        incomeStatement.otherCost += -cost.transactionAmount
            '    End If
            'Next

            incomeStatement.totalProfit = incomeStatement.totalRevenue - incomeStatement.totalCost - incomeStatement.rndCost


            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(incomeStatement), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try
        Return returnedMessage
    End Function

    Friend Function changeFirmRndExpenditure(firmId As Integer, yearlyExpenditure As Decimal?, userId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim firm As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If firm Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified firm")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim user As USER = (From query In databaseEntities.USERs Where query.ID = userId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If user Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified user")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim period As PERIOD = findCurrentActivePeriod(firmId, databaseEntities)
            If period Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "There is no currently active priod in play for the command to take effect")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            If yearlyExpenditure Is Nothing Then
                yearlyExpenditure = 0
            End If

            Dim rndDecision As New RND_DECISIONS
            rndDecision.GAME_FIRM = firm
            rndDecision.USER = user
            rndDecision.PERIOD = period
            rndDecision.IS_DELETED = False
            databaseEntities.RND_DECISIONS.Add(rndDecision)

            Dim revenueAndCost As New REVENUE_AND_COST
            revenueAndCost.FLOW_TYPE = CashflowType.cost
            revenueAndCost.PAYMENT_TYPE = PaymentType.monthlyRecursive
            revenueAndCost.RECURSIVE_DURATION = 0
            revenueAndCost.PAYMENT_AMOUNT = yearlyExpenditure / 12
            revenueAndCost.NAME = "RndMonthlyCost"
            revenueAndCost.GAME_FIRM = firm
            revenueAndCost.PERIOD = period
            revenueAndCost.IS_DELETED = False
            revenueAndCost.TARGET_TYPE = PaymentTargetType.equity
            revenueAndCost.FIRST_PAYMENT_DATE = 1
            revenueAndCost.BUSINESS_ASPECT = BusinessAspect.rnd
            databaseEntities.REVENUE_AND_COST.Add(revenueAndCost)

            rndDecision.REVENUE_AND_COST = revenueAndCost

            Dim currentRndRevenueAndCost As REVENUE_AND_COST = findCurrentFirmRndRevenueAndCost(firm.ID, period.ID)
            If currentRndRevenueAndCost IsNot Nothing Then
                ' unsubsribeRevenueAndcost(currentRndRevenueAndCost.ID)
                currentRndRevenueAndCost.IS_DELETED = True
            End If
            databaseEntities.SaveChanges()

            ' subscribeRevenueAndCost(revenueAndCost.ID)
        Catch ex As Exception
            Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try


        Return returnedMessage
    End Function

    Friend Function changeFirmProductPrice(firmId As Integer, price As Decimal, userId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim firm As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If firm Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified firm")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim user As USER = (From query In databaseEntities.USERs Where query.ID = userId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If user Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified user")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim period As PERIOD = findCurrentActivePeriod(firmId, databaseEntities)
            If period Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "There is no currently active priod in play for the command to take effect")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim marketingDecision As New MARKETING_DECISIONS
            marketingDecision.GAME_FIRM = firm
            marketingDecision.USER = user
            marketingDecision.PERIOD = period
            marketingDecision.IS_DELETED = False
            marketingDecision.PRICE = Decimal.Parse(price)

            Dim numberOfSecondForOneDay = (Date.Parse(period.EXPECTED_END) - Date.Parse(period.EXPECTED_START)).TotalSeconds / period.SIMULATION_DURATION
            marketingDecision.IN_GAME_TIME_IN_DAY = (Date.Now - Date.Parse(period.START_TIME)).TotalSeconds / numberOfSecondForOneDay
            databaseEntities.MARKETING_DECISIONS.Add(marketingDecision)
            firm.PRODUCTION_PRICE = price

            Dim marketInfo As MARKET_INFO = firm.MARKET_INFO
            Dim alpha As Double = marketInfo.ALPHA
            Dim beta = -0.3
            Dim firmList = marketInfo.GAME_FIRM.ToList
            Dim denominator As Double = 0
            For Each otherFirm In firmList
                denominator += Math.Exp(beta * otherFirm.PRODUCTION_PRICE)
            Next

            Dim marketShareProportion = Math.Exp(beta * firm.PRODUCTION_PRICE) / denominator
            Dim marketShareQuantity = marketShareProportion * marketInfo.MARKET_SIZE
            'Dim result As New RESULT
            'result.ACTION_FIRM_LINK_ID = firm.ID
            'result.TARGET_FIRM_ID = firm.ID
            'Dim currentShareQty = firm.MARKET_SHARE_QTY
            'If currentShareQty Is Nothing Then
            '    currentShareQty = 0
            'End If
            'result.MARKET_SHARE_CHANGE = marketShareQuantity - currentShareQty
            'result.MARKETING_DECISIONS = marketingDecision
            'databaseEntities.RESULTs.Add(result)

            'firm.MARKET_SHARE = marketShareProportion
            'firm.MARKET_SHARE_QTY = marketShareQuantity

            For Each otherFirm In firmList
                Dim newMarketShareProportion = Math.Exp(beta * otherFirm.PRODUCTION_PRICE) / denominator
                Dim newMarketShareQuantity = newMarketShareProportion * marketInfo.MARKET_SIZE
                'Dim previousResult As RESULT = (From query In databaseEntities.RESULTs Where query.TARGET_FIRM_ID = otherFirm.ID AndAlso query.ACTION_FIRM_LINK_ID = otherFirm.ID
                '                                Order By query.ID Descending).FirstOrDefault
                Dim newResult As New RESULT
                newResult.ACTION_FIRM_LINK_ID = firm.ID
                newResult.TARGET_FIRM_ID = otherFirm.ID
                Dim newCurrentShareQty = otherFirm.MARKET_SHARE_QTY
                If newCurrentShareQty Is Nothing Then
                    newCurrentShareQty = 0
                End If
                'Dim previousMarketShareQuantity = otherFirm.MARKET_SHARE_QTY - previousResult.MARKET_SHARE_CHANGE
                newResult.MARKET_SHARE_CHANGE = newMarketShareQuantity - newCurrentShareQty
                newResult.MARKETING_DECISIONS = marketingDecision
                databaseEntities.RESULTs.Add(newResult)
                otherFirm.MARKET_SHARE = newMarketShareProportion
                ' otherFirm.MARKET_SHARE_QTY = (alpha * newMarketShareQuantity) + (1 - alpha) * (previousMarketShareQuantity)
                otherFirm.MARKET_SHARE_QTY = newMarketShareQuantity
            Next

            databaseEntities.SaveChanges()
        Catch ex As Exception
            Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function changeFirmProductionQuantity(firmId As Integer, quantity As Decimal, userId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim firm As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If firm Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified firm")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim user As USER = (From query In databaseEntities.USERs Where query.ID = userId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If user Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "Unable to find the specified user")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim period As PERIOD = findCurrentActivePeriod(firmId, databaseEntities)
            If period Is Nothing Then
                Dim wrapperObject = New BaseModel(Enums.notFound, "There is no currently active priod in play for the command to take effect")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            Dim productionDecision As New PRODUCTION_DECISIONS
            productionDecision.GAME_FIRM = firm
            productionDecision.PERIOD = period
            productionDecision.USER = user
            productionDecision.QUANTITY = quantity
            productionDecision.IS_DELETED = False
            Dim numberOfSecondForOneDay = (Date.Parse(period.EXPECTED_END) - Date.Parse(period.EXPECTED_START)).TotalSeconds / period.SIMULATION_DURATION
            productionDecision.IN_GAME_TIME_IN_DAY = (Date.Now - Date.Parse(period.START_TIME)).TotalSeconds / numberOfSecondForOneDay
            databaseEntities.PRODUCTION_DECISIONS.Add(productionDecision)

            Dim revenueAndCost As New REVENUE_AND_COST
            revenueAndCost.FLOW_TYPE = CashflowType.cost
            revenueAndCost.PAYMENT_TYPE = PaymentType.oneTime
            revenueAndCost.RECURSIVE_DURATION = 0
            revenueAndCost.PAYMENT_AMOUNT = quantity * firm.PRODUCTION_COST
            revenueAndCost.NAME = "ProductionCost"
            revenueAndCost.GAME_FIRM = firm
            revenueAndCost.PERIOD = period
            revenueAndCost.IS_DELETED = False
            revenueAndCost.TARGET_TYPE = PaymentTargetType.asset
            revenueAndCost.FIRST_PAYMENT_DATE = productionDecision.IN_GAME_TIME_IN_DAY
            revenueAndCost.BUSINESS_ASPECT = BusinessAspect.production
            databaseEntities.REVENUE_AND_COST.Add(revenueAndCost)

            productionDecision.REVENUE_AND_COST = revenueAndCost
            databaseEntities.SaveChanges()
            ' subscribeRevenueAndCost(revenueAndCost.ID)

        Catch ex As Exception
            Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Private Function findCurrentFirmRndRevenueAndCost(firmId As Integer, periodId As Integer) As REVENUE_AND_COST
        Try
            Dim currentRndRevenueAndCost As REVENUE_AND_COST = (From query In getDatabaseEntity.REVENUE_AND_COST Where query.FIRM_LINK_ID = firmId AndAlso query.PERIOD_LINK_ID = periodId AndAlso
                                                                                                                     (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            Return currentRndRevenueAndCost
        Catch ex As Exception
            Return Nothing
        End Try
    End Function


    Private Sub ReturnMarketShare(ByRef firmEntity As GAME_FIRM)
        Dim marketInfo As MARKET_INFO = firmEntity.MARKET_INFO
        Dim alpha As Double = marketInfo.ALPHA
        Dim firmList = marketInfo.GAME_FIRM.ToList
        Dim denominator As Double = 0
        For Each firm In firmList
            denominator += Math.Exp(alpha * firm.PRODUCTION_PRICE)
        Next
        Dim marketShareProportion = Math.Exp(alpha * firmEntity.PRODUCTION_PRICE) / denominator
        Dim marketShareQuantity = marketShareProportion * marketInfo.MARKET_SIZE
        firmEntity.MARKET_SHARE = marketShareProportion
        firmEntity.MARKET_SHARE_QTY = marketShareQuantity
    End Sub

    Friend Function addCosting(costModel As CostModel, firmId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim costing As New REVENUE_AND_COST
            Dim firmEntity As GAME_FIRM = (From query In databaseEntities.GAME_FIRM Where query.ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
            If firmEntity IsNot Nothing Then
                costing.GAME_FIRM = firmEntity
            End If

            costing.IS_DELETED = False
            costing.FLOW_TYPE = costModel.cashflowType
            costing.PAYMENT_TYPE = costModel.paymentType
            costing.TARGET_TYPE = costModel.paymentTargetType
            costing.BUSINESS_ASPECT = costModel.businessAspect
            costing.PAYMENT_AMOUNT = costModel.paymentAmount
            costing.NAME = costModel.name
            costing.DESCRIPTION = costModel.description
            costing.RECURSIVE_DURATION = costModel.numberOfPayment
            costing.FIRST_PAYMENT_DATE = costModel.firstPaymentDate
            costing.PERIOD_NUMBER = costModel.periodNumber

            databaseEntities.REVENUE_AND_COST.Add(costing)
            databaseEntities.SaveChanges()

            Dim wrapperObject = New BaseModel(Enums.success, "")
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getCostingByFirmId(firmId As Integer) As HttpResponseMessage
        Dim databaseEntities = getDatabaseEntity()
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim costingList As List(Of REVENUE_AND_COST) = (From query In databaseEntities.REVENUE_AND_COST Where query.FIRM_LINK_ID = firmId AndAlso (query.IS_DELETED Is Nothing OrElse
                                                                                                                query.IS_DELETED = False)).ToList()
            If costingList IsNot Nothing Then
                Dim wrapperObject As New CostListModel(Enums.success, "")
                For Each costing In costingList
                    wrapperObject.costingList.Add(mapCosting(costing))
                Next
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            Else
                Dim wrapperObject As New CostListModel(Enums.notFound, "Unable To retrieve the costing List")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
        Catch ex As Exception
            Dim wrapperObject As New CostListModel(Enums.fail, "An error has occured: " + Environment.NewLine + ex.Message)
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Private Function mapCosting(costing As REVENUE_AND_COST) As CostModel
        Dim costModel As New CostModel(Enums.success, "")
        costModel.id = costing.ID
        costModel.cashflowType = costing.FLOW_TYPE
        costModel.paymentType = costing.PAYMENT_TYPE
        costModel.paymentTargetType = costing.TARGET_TYPE
        costModel.businessAspect = costing.BUSINESS_ASPECT
        costModel.paymentAmount = costing.PAYMENT_AMOUNT
        costModel.name = costing.NAME
        costModel.description = costing.DESCRIPTION
        costModel.numberOfPayment = costing.RECURSIVE_DURATION
        costModel.firstPaymentDate = costing.FIRST_PAYMENT_DATE
        costModel.periodNumber = costing.PERIOD_NUMBER

        Return costModel
    End Function

    Private Function mapFirm(firmInfo As GAME_FIRM) As FirmModel
        Dim firmModel As New FirmModel(Enums.success, "")
        firmModel.id = firmInfo.ID
        firmModel.firmDescription = firmInfo.DESCRIPTION
        firmModel.firmName = firmInfo.FIRM_NAME
        firmModel.gameLinkId = firmInfo.GAME_LINK_ID
        firmModel.marketShare = firmInfo.MARKET_SHARE_QTY
        firmModel.marketSharePercentage = firmInfo.MARKET_SHARE
        firmModel.maxNumberOfPlayers = firmInfo.MAX_NUMBER_OF_PLAYER
        firmModel.productionPrice = firmInfo.PRODUCTION_PRICE
        firmModel.productionQuality = firmInfo.PRODUCTION_QUALITY

        Return firmModel
    End Function

    Friend Function getListOfGameByUserId(userId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim availableGameList As List(Of GAME) = getAvailableGameList(userId)
            Dim joinedGameList As List(Of GAME) = getJoinedGameList(userId)
            Dim allGameList As List(Of GAME) = (From query In getDatabaseEntity.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList

            Dim availableGameModelList As New List(Of GameModel)
            Dim joinedGameModelList As New List(Of GameModel)

            For Each game In availableGameList
                availableGameModelList.Add(mapGame(game))
            Next

            For Each game In joinedGameList
                Dim gameModel As GameModel = mapGame(game)

                Dim currentFirm As GAME_FIRM = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                                                          query.USER_LINK_ID = userId AndAlso query.GAME_LINK_ID = game.ID AndAlso query.FIRM_LINK_ID IsNot Nothing Select query.GAME_FIRM).FirstOrDefault
                If currentFirm IsNot Nothing Then
                    gameModel.joinedFirmId = currentFirm.ID
                End If
                joinedGameModelList.Add(gameModel)
            Next

            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New GameListModel(Enums.success, "")
            wrapperObject.availableGameList = availableGameModelList
            wrapperObject.joinedGameList = joinedGameModelList
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading list of available games.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Private Function getAvailableGameList(userId As Integer, Optional gameTypeId As Integer? = Nothing) As List(Of GAME)
        Dim availableGameList As New List(Of GAME)
        Dim joinedGameList As List(Of GAME) = getJoinedGameList(userId, gameTypeId)
        Dim allGameList As List(Of GAME)

        If gameTypeId Is Nothing Then
            allGameList = (From query In entities.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
        Else
            allGameList = (From query In entities.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso query.GAME_TYPE_LINK_ID = gameTypeId).ToList
        End If

        For Each game In allGameList
            If Not joinedGameList.Select(Function(x)
                                             Return x.ID
                                         End Function).ToList().Contains(game.ID) Then
                availableGameList.Add(game)
            End If
        Next

        Return availableGameList
    End Function

    Private Function getJoinedGameList(userId As Integer, Optional gameTypeId As Integer? = Nothing) As List(Of GAME)
        Dim joinedGameList As List(Of GAME)
        If gameTypeId Is Nothing Then
            joinedGameList = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                                                         query.USER_LINK_ID = userId AndAlso query.GAME IsNot Nothing Select query.GAME).ToList
        Else
            joinedGameList = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                                                         query.USER_LINK_ID = userId AndAlso query.GAME IsNot Nothing AndAlso
                                                                                                        query.GAME.GAME_TYPE_LINK_ID = gameTypeId Select query.GAME).ToList
        End If
        joinedGameList = joinedGameList.Distinct.ToList
        joinedGameList.RemoveAll(Function(x)
                                     If x.IS_DELETED = True Then
                                         Return True
                                     Else
                                         Return false
                                     End If
                                 End Function)
        Return joinedGameList
    End Function

    Friend Function enlistUserToGame(userId As Integer, gameId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage
        entities = getDatabaseEntity()

        Try
            Dim availableGameList As List(Of GAME) = getAvailableGameList(userId)
            Dim gameToJoin As GAME = availableGameList.Find(Function(x)
                                                                If x.ID = gameId Then
                                                                    Return True
                                                                Else Return False
                                                                End If
                                                            End Function)
            If gameToJoin IsNot Nothing Then
                Dim relationship As New USER_USER_ROLE_GAME_FIRM_RELATIONSHIP
                relationship.IS_DELETED = False
                relationship.USER_LINK_ID = userId
                relationship.GAME_LINK_ID = gameId

                Dim firmToJoin As GAME_FIRM = findAvailableFirm(gameToJoin)

                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New GameModel(Enums.success, "")
                wrapperObject = mapGame(gameToJoin)

                If firmToJoin IsNot Nothing Then
                    relationship.GAME_FIRM = firmToJoin
                    entities.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP.Add(relationship)
                    entities.SaveChanges()
                    wrapperObject.joinedFirmId = firmToJoin.ID
                Else
                    wrapperObject.responseCode = Enums.fail
                    wrapperObject.responseMessage = "Unable to find an available firm to enlist user to in the game"
                End If

                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            Else
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.fail, "The user has already joined the game or the game is not available.")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while enlisting the user to the specified game.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Private Function findAvailableFirm(gameToJoin As GAME) As GAME_FIRM
        Dim firmList As List(Of GAME_FIRM) = gameToJoin.GAME_FIRM.ToList
        Dim returnedGameFirm As GAME_FIRM = Nothing
        If firmList IsNot Nothing AndAlso firmList.Count > 0 Then
            returnedGameFirm = firmList.Find(Function(x)
                                                 If (x.IS_DELETED Is Nothing OrElse x.IS_DELETED = False) AndAlso x.MAX_NUMBER_OF_PLAYER > getFirmCurrentNumberOfPlayers(x) Then
                                                     Return True
                                                 Else
                                                     Return False
                                                 End If
                                             End Function)
        End If
        Return returnedGameFirm
    End Function

    Private Function getFirmCurrentNumberOfPlayers(firm As GAME_FIRM) As Integer?
        Dim relationshipList As List(Of USER_USER_ROLE_GAME_FIRM_RELATIONSHIP) = firm.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP.ToList
        Dim userIdList As New List(Of Integer)
        For Each relationship In relationshipList
            If relationship.IS_DELETED Is Nothing OrElse relationship.IS_DELETED = False Then
                If relationship.USER_LINK_ID IsNot Nothing AndAlso Not userIdList.Contains(relationship.USER_LINK_ID) Then
                    userIdList.Add(relationship.USER_LINK_ID)
                End If
            End If
        Next
        Return userIdList.Count
    End Function

    Friend Function getListOfGame() As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim gameTypeList As List(Of GAME) = (From query In getDatabaseEntity.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).ToList
            Dim returnedModelList As New List(Of GameModel)
            For Each game In gameTypeList
                returnedModelList.Add(mapGame(game))
            Next

            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New GameListModel(Enums.success, "")
            wrapperObject.availableGameList = returnedModelList
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading list of available game types.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getListOfGameByType(gameTypeId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim gameTypeList As List(Of GAME) = (From query In getDatabaseEntity.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                           query.GAME_TYPE_LINK_ID = gameTypeId).ToList
            Dim returnedModelList As New List(Of GameModel)
            For Each game In gameTypeList
                returnedModelList.Add(mapGame(game))
            Next

            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New GameListModel(Enums.success, "")
            wrapperObject.availableGameList = returnedModelList
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading list of available game types.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getGameById(gameId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim game As GAME = (From query In getDatabaseEntity.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                           query.ID = gameId).FirstOrDefault
            If game IsNot Nothing Then
                Dim gameModel = mapGame(game)
                gameModel.responseCode = Enums.success
                gameModel.responseMessage = ""
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(gameModel), Text.Encoding.UTF8, "application/json")
            Else
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.notFound, "Unable to find the specified game.")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading list of available game types.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getPeriodByGameId(gameId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim gamePeriod As PERIOD
            Dim periodList = (From query In getDatabaseEntity.PERIODs Where query.GAME_LINK_ID = gameId AndAlso query.PERIOD_NUMBER IsNot Nothing AndAlso
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
                gamePeriod = periodList.Find(Function(x)
                                                 If x.START_TIME IsNot Nothing AndAlso x.END_TIME Is Nothing Then
                                                     Return True
                                                 Else
                                                     Return False
                                                 End If
                                             End Function)
                If gamePeriod Is Nothing Then
                    gamePeriod = periodList.Find(Function(x)
                                                     If x.START_TIME Is Nothing AndAlso x.END_TIME Is Nothing Then
                                                         Return True
                                                     Else
                                                         Return False
                                                     End If
                                                 End Function)
                    If gamePeriod Is Nothing Then
                        gamePeriod = periodList.Last()
                    End If
                End If
            End If

            If gamePeriod IsNot Nothing Then
                Dim gamePeriodModel = mapPeriod(gamePeriod)
                gamePeriodModel.responseCode = Enums.success
                gamePeriodModel.responseMessage = ""
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(gamePeriodModel), Text.Encoding.UTF8, "application/json")
            Else
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.notFound, "Unable to find the specified game period.")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading the game period.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function

    Friend Function getBalanceSheetByGameId(gameFirmId As Integer) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage

        Try
            Dim balanceSheet As BALANCE_SHEET = (From query In getDatabaseEntity.BALANCE_SHEET Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                           query.GAME_FIRM.FirstOrDefault.ID = gameFirmId).FirstOrDefault
            If balanceSheet IsNot Nothing Then
                Dim gamePeriodModel = mapBalanceSheet(balanceSheet)
                gamePeriodModel.responseCode = Enums.success
                gamePeriodModel.responseMessage = ""
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(gamePeriodModel), Text.Encoding.UTF8, "application/json")
            Else
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.notFound, "Unable to find the specified balance sheet.")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
        Catch ex As Exception
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.fail, "An error occured while loading the reequired balance sheet.")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End Try

        Return returnedMessage
    End Function
End Module
