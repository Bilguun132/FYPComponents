Imports System.Windows.Input
Imports ContentManager
Imports GameAdministratorCenter.Contracts

Public Class GameTypeView
    Private selectedGameType As GAME_TYPE = Nothing
    Public Delegate Sub gameTypeSelected(gameType As GAME_TYPE)
    Public gameTypeSelectedDelegate As gameTypeSelected = Nothing

    Public Sub New(gameType As GAME_TYPE)

        ' This call is required by the designer.
        InitializeComponent()
        selectedGameType = gameType
        ' Add any initialization after the InitializeComponent() call.
        gameTypeName.Text = gameType.GAME_TYPE_NAME
        If gameType.IMAGE_MANAGER_LINK_ID IsNot Nothing Then
            Dim viewer = New ImageViewer(Integer.Parse(gameType.IMAGE_MANAGER_LINK_ID))
            viewer.hideButtonPanel()
            viewer.disableImageManipulation()
            imageContainer.Content = viewer
        End If
    End Sub

    Private Sub mainLayoutGroup_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles mainLayoutGroup.MouseUp
        If gameTypeSelectedDelegate IsNot Nothing AndAlso selectedGameType IsNot Nothing Then
            gameTypeSelectedDelegate(selectedGameType)
        End If
    End Sub
End Class
