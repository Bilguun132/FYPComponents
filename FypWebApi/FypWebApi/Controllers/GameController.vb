Imports System.Net
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.Http
Imports GameAdministratorCenter.Contracts
Imports Newtonsoft.Json

Namespace Controllers
    <RoutePrefix("api")>
    Public Class GameController
        Inherits ApiController

        <Route("games")>
        <HttpGet>
        Public Function getGameList() As HttpResponseMessage
            Return GameService.getListOfGame()
        End Function

        <Route("games/gameType/{gameTypeId}")>
        <HttpGet>
        Public Function getGameListByTypeId(gameTypeId As Integer) As HttpResponseMessage
            Return GameService.getListOfGameByType(gameTypeId)
        End Function

        <Route("games/gameType/{gameTypeId}/user/{userId}")>
        <HttpGet>
        Public Function getGameListByTypeId(gameTypeId As Integer, userId As Integer) As HttpResponseMessage
            Return GameService.getListOfGameByTypeAndUserId(gameTypeId, userId)
        End Function

        <Route("games/{gameId}")>
        <HttpGet>
        Public Function getGameById(gameId As Integer) As HttpResponseMessage
            Return GameService.getGameById(gameId)
        End Function

        <Route("games/period/{gameId}")>
        <HttpGet>
        Public Function getPeriodByGameId(gameId As Integer) As HttpResponseMessage
            Return GameService.getPeriodByGameId(gameId)
        End Function

        <Route("games/balanceSheet/{firmId}")>
        <HttpGet>
        Public Function getBalanceSheetByGameId(firmId As Integer) As HttpResponseMessage
            Return GameService.getBalanceSheetByGameId(firmId)
        End Function

        <Route("games/costing/{firmId}")>
        <HttpGet>
        Public Function getFirmCosting(firmId As Integer) As HttpResponseMessage
            Return GameService.getCostingByFirmId(firmId)
        End Function

        <Route("games/firms/productionChange/{firmId}")>
        <HttpPost>
        Public Function changeProductionParameters(firmId As Integer) As HttpResponseMessage
            Dim returnedMessage As New HttpResponseMessage
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            Dim quality = content.Result.Item("quality")
            Dim price = content.Result.Item("price")
            Dim newEntity = DatabaseService.getDatabaseEntity
            Dim firm = newEntity.GAME_FIRM.Where(Function(p) p.ID = firmId).FirstOrDefault
            If firm IsNot Nothing Then
                If quality IsNot Nothing Then
                    firm.PRODUCTION_QUALITY = quality
                End If
                If price IsNot Nothing Then
                    firm.PRODUCTION_PRICE = price
                End If
                newEntity.SaveChanges()
                Dim wrapperObject = New BaseModel(Enums.success, "")
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            Else
                Dim wrapperObject = New BaseModel(Enums.fail, "An error has occured" + Environment.NewLine)
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
            Return returnedMessage
        End Function

        <Route("games/firms/{firmId}/productionDecision")>
        <HttpPost>
        Public Function changeProductionQuantity(firmId As Integer) As HttpResponseMessage
            Dim returnedMessage As New HttpResponseMessage
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            Dim quantity = Decimal.Parse(content.Result.Item("productionQuantity"))
            Dim userId = Integer.Parse(content.Result.Item("userId"))
            Return GameService.changeFirmProductionQuantity(firmId, quantity, userId)
        End Function

        <Route("games/firms/{firmId}/marketingDecision")>
        <HttpPost>
        Public Function changeFirmProductPrice(firmId As Integer) As HttpResponseMessage
            Dim returnedMessage As New HttpResponseMessage
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            Dim userId = Integer.Parse(content.Result.Item("userId"))
            Dim price = Decimal.Parse(content.Result.Item("productPrice"))
            Return GameService.changeFirmProductPrice(firmId, price, userId)
        End Function

        <Route("games/firms/{firmId}/incomeStatement/{periodId}")>
        <HttpGet>
        Public Function getFirmIncomeStatementByPeriod(firmId As Integer, periodId As Integer) As HttpResponseMessage
            Return GameService.getFirmIncomeStatement(firmId, periodId)
        End Function

        <Route("games/firms/{firmId}/rndDecision")>
        <HttpPost>
        Public Function changeFirmRndBudget(firmId As Integer) As HttpResponseMessage
            Dim returnedMessage As New HttpResponseMessage
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            Dim userId = Integer.Parse(content.Result.Item("userId"))
            Dim yearlyExpenditure = Decimal.Parse(content.Result.Item("annualExpenditure"))
            Return GameService.changeFirmRndExpenditure(firmId, yearlyExpenditure, userId)
        End Function

        <Route("games/firms/{firmId}")>
        <HttpGet>
        Public Function getFirmInfo(firmId As Integer) As HttpResponseMessage
            Return GameService.getFirmInfobyFirmId(firmId)
        End Function

        <Route("games/costing/{firmId}/add")>
        <HttpPost>
        Public Function createfirmCosting(firmId As Integer) As HttpResponseMessage
            Dim costModel As New CostModel(Enums.success, "")
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            costModel.cashflowType = content.Result.Item("cashflowType")
            costModel.paymentType = content.Result.Item("paymentType")
            costModel.paymentTargetType = content.Result.Item("paymentTargetType")
            costModel.businessAspect = content.Result.Item("businessAspect")
            costModel.paymentAmount = content.Result.Item("paymentAmount")
            costModel.name = content.Result.Item("name")
            costModel.description = content.Result.Item("description")
            costModel.numberOfPayment = content.Result.Item("numberOfPayment")
            costModel.firstPaymentDate = content.Result.Item("firstPaymentDate")
            costModel.periodNumber = content.Result.Item("periodNumber")

            Return GameService.addCosting(costModel, firmId)
        End Function
    End Class
End Namespace