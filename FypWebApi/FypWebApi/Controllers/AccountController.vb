Imports System.Net
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.Http

Namespace Controllers
    <RoutePrefix("api/account")>
    Public Class AccountController
        Inherits ApiController

        <Route("register")>
        <HttpPost>
        Public Function registerNewAccount() As HttpResponseMessage
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            Dim userName = content.Result.Item("username")
            Dim email = content.Result.Item("email")
            Dim password = content.Result.Item("password")
            Return AccountService.registerNewAccount(userName, email, password)
        End Function

        <Route("login")>
        <HttpPost>
        Public Function validateLogin() As HttpResponseMessage
            Dim content As Task(Of NameValueCollection) = Request.Content.ReadAsFormDataAsync
            Dim username = content.Result.Item("username")
            Dim email = content.Result.Item("email")
            Dim password = content.Result.Item("password")
            Return AccountService.loginAccount(username, email, password)
        End Function

        <Route("{userId}/games")>
        <HttpGet>
        Public Function getUserGames(userId As Integer) As HttpResponseMessage
            Return GameService.getListOfGameByUserId(userId)
        End Function

        <Route("{userId}/games/{gameId}/join")>
        <HttpGet>
        Public Function joinGame(userId As Integer, gameId As Integer) As HttpResponseMessage
            Return GameService.enlistUserToGame(userId, gameId)
        End Function
    End Class
End Namespace