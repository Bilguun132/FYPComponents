Imports GameAdministratorCenter.Contracts

Public Module MapperService
    Public Function mapGameType(gameType As GAME_TYPE) As GameTypeModel
        Dim returnedModel As New GameTypeModel(Nothing, Nothing)
        returnedModel.id = gameType.ID
        returnedModel.gameTypeName = gameType.GAME_TYPE_NAME

        If gameType.IMAGE_MANAGER_LINK_ID IsNot Nothing Then
            If gameType.CONTENT_MANAGER.CONTENT_THUMBNAIL IsNot Nothing Then
                returnedModel.gameTypeImageString = gameType.CONTENT_MANAGER.CONTENT_THUMBNAIL
            ElseIf gameType.CONTENT_MANAGER.IMAGE_STRING IsNot Nothing Then
                returnedModel.gameTypeImageString = gameType.CONTENT_MANAGER.IMAGE_STRING.IMAGE_HEX_STRING
            End If
        End If
        returnedModel.gameTypeDescription = gameType.DESCRIPTION
        Return returnedModel
    End Function


    Public Function mapGame(game As GAME) As GameModel
        Dim returnedModel As New GameModel(Nothing, Nothing)

        returnedModel.id = game.ID
        returnedModel.gameDescription = game.DESCRIPTION
        returnedModel.gameName = game.GAME_NAME
        returnedModel.gamePassword = game.GAME_PASSWORD
        If game.GAME_TYPE IsNot Nothing Then
            returnedModel.gameTypeId = game.GAME_TYPE_LINK_ID
            returnedModel.gameType = game.GAME_TYPE.GAME_TYPE_NAME
        End If

        If game.IMAGE_MANAGER_LINK_ID IsNot Nothing Then
            If game.CONTENT_MANAGER.CONTENT_THUMBNAIL IsNot Nothing Then
                returnedModel.gameImageString = game.CONTENT_MANAGER.CONTENT_THUMBNAIL
            ElseIf game.CONTENT_MANAGER.IMAGE_STRING IsNot Nothing Then
                returnedModel.gameImageString = game.CONTENT_MANAGER.IMAGE_STRING.IMAGE_HEX_STRING
            End If
        End If

        Return returnedModel
    End Function

    Public Function mapPeriod(period As PERIOD) As GamePeriodModel
        Dim returnedModel As New GamePeriodModel(Nothing, Nothing)
        With returnedModel
            .id = period.ID
            .periodName = period.PERIOD_NAME
            .periodNumber = If(period.PERIOD_NUMBER IsNot Nothing, period.PERIOD_NUMBER, -1)
            .status = If(period.STATUS IsNot Nothing, period.STATUS, 0)
            .expectedEnd = period.EXPECTED_END
            .expectedStart = period.EXPECTED_START
            .gameLinkId = period.GAME_LINK_ID
            .startTime = If(period.START_TIME IsNot Nothing, period.START_TIME, Nothing)
            .endTime = If(period.END_TIME IsNot Nothing, period.END_TIME, Nothing)
        End With

        Dim previousPeriod As PERIOD = (From query In getDatabaseEntity.PERIODs Where query.PERIOD_NUMBER = period.PERIOD_NUMBER - 1 AndAlso query.GAME_LINK_ID = period.GAME_LINK_ID AndAlso
                                                                                     (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault
        If previousPeriod IsNot Nothing Then
            returnedModel.previousPeriodId = previousPeriod.ID
        End If
        Return returnedModel
    End Function

    Public Function mapBalanceSheet(balanceSheet As BALANCE_SHEET) As BalanceSheetModel
        Dim returnedModel As New BalanceSheetModel(Nothing, Nothing)
        With returnedModel
            .id = balanceSheet.ID
            .totalAsset = balanceSheet.TOTAL_ASSET
            .totalEquity = balanceSheet.TOTAL_EQUITY
            .totalLiability = balanceSheet.TOTAL_LIABILITY
        End With
        Return returnedModel
    End Function

    Public Function mapMarketingDecision(marketingDecision As MARKETING_DECISIONS) As MarketingDecisionModel
        Dim returnedModel As New MarketingDecisionModel(Nothing, Nothing)
        With returnedModel
            .id = marketingDecision.ID
            .price = marketingDecision.PRICE
            .setDateInGame = marketingDecision.IN_GAME_TIME_IN_DAY
            .firmLinkId = marketingDecision.FIRM_LINK_ID
            .userLinkId = marketingDecision.USER_LINK_ID
            .periodLinkId = marketingDecision.PERIOD_LINK_ID
            .marketShareQtyChange = marketingDecision.RESULTs.Where(Function(p) p.TARGET_FIRM_ID = marketingDecision.GAME_FIRM.ID).FirstOrDefault.MARKET_SHARE_CHANGE
        End With
        Return returnedModel
    End Function

    Public Function mapProductionDecision(productionDecision As PRODUCTION_DECISIONS) As ProductionDecisionModel
        Dim returnedModel As New ProductionDecisionModel(Nothing, Nothing)
        With returnedModel
            .id = productionDecision.ID
            .productionQuantity = productionDecision.QUANTITY
            .firmLinkId = productionDecision.FIRM_LINK_ID
            .periodLinkId = productionDecision.PERIOD_LINK_ID
            .setDateInGame = productionDecision.IN_GAME_TIME_IN_DAY
            .userLinkId = productionDecision.USER_LINK_ID
            .revenueAndCostLinkId = productionDecision.REVENUE_AND_COST_LINK_ID
        End With
        Return returnedModel
    End Function

    Public Function mapTransaction(transaction As BALANCE_SHEET_TRANSACTION) As BalanceTransactionModel
        Dim returnedModel As New BalanceTransactionModel(Nothing, Nothing)
        With returnedModel
            .id = transaction.ID
            .businessAspect = getBusinessAspectFromEnum(transaction.TRANSACTION_STATUS)
            .balanceSheetLinkId = transaction.BALANCE_SHEET_LINK_ID
            .transactionAmount = transaction.TRANSACTION_AMOUNT
            .transactionName = transaction.TRANSACTION_NAME
            .targetType = getTargetTypeFromEnum(transaction.TRANSACTION_TYPE)
            .transactionJson = transaction.TRANSACTION_JSON
            .dateTimeInserted = transaction.DATE_TIME_INSERTED
            .revenueAndCostLinkId = transaction.REVENUE_AND_COST_LINK_ID
        End With
        Return returnedModel
    End Function

    Private Function getTargetTypeFromEnum(transactionType As Integer?) As String
        If transactionType Is Nothing Then
            Return ""
        Else
            Select Case Integer.Parse(transactionType)
                Case PaymentTargetType.asset
                    Return "Total Asset"
                Case PaymentTargetType.equity
                    Return "Total Equity"
                Case PaymentTargetType.liability
                    Return "Total Liability"
                Case Else
                    Return ""
            End Select
        End If
    End Function

    Private Function getBusinessAspectFromEnum(transactionStatus As Integer?) As String
        If transactionStatus Is Nothing Then
            Return ""
        Else
            Select Case Integer.Parse(transactionStatus)
                Case BusinessAspect.marketing
                    Return "Marketing"
                Case BusinessAspect.production
                    Return "Production"
                Case BusinessAspect.rnd
                    Return "Research & Development"
                Case Else
                    Return ""
            End Select
        End If
    End Function
End Module
