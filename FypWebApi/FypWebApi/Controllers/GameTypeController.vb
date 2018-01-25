Imports System.Net
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Web.Http

Namespace Controllers
    <RoutePrefix("api")>
    Public Class GameTypeController
        Inherits ApiController

        <Route("gameTypes")>
        <HttpGet>
        Public Function getGameTypeList() As HttpResponseMessage
            Return GameService.getListOfGameType()
        End Function
    End Class
End Namespace