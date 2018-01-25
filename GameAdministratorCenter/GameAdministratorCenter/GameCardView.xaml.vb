Imports System.Windows
Imports System.Windows.Input
Imports ContentManager
Imports GameAdministratorCenter.Contracts

Public Class GameCardView
    Public Property selectedGame As GAME

    Public Delegate Sub gameSelected(game As GAME)
    Public gameSelectedDelegate As gameSelected = Nothing

    Public Sub New(game As GAME)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        selectedGame = game
    End Sub

    Private Sub GameCardView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If selectedGame IsNot Nothing Then
            gameNameTextBlock.Text = selectedGame.GAME_NAME
            If selectedGame.IMAGE_MANAGER_LINK_ID IsNot Nothing Then
                Dim viewer As New ImageViewer(Integer.Parse(selectedGame.IMAGE_MANAGER_LINK_ID))
                viewer.hideButtonPanel()
                viewer.disableImageManipulation()
                imageContainer.Content = viewer
            End If

            If selectedGame.STATUS IsNot Nothing Then
                Select Case selectedGame.STATUS
                    Case ProgressStatusEnum.notStarted
                        statusTextblock.Text = "Not Started"
                        statusImage.Source = convertBitmapToBitmapSource(My.Resources.Orange_circle)
                    Case ProgressStatusEnum.inProgress
                        statusTextblock.Text = "In Progress"
                        statusImage.Source = convertBitmapToBitmapSource(My.Resources.Green_circle)
                    Case ProgressStatusEnum.completed
                        statusTextblock.Text = "Completed"
                        statusImage.Source = convertBitmapToBitmapSource(My.Resources.Blue_circle)
                End Select
            Else
                statusTextblock.Text = "Not Started"
                statusImage.Source = convertBitmapToBitmapSource(My.Resources.Orange_circle)
            End If

            Dim periodList As List(Of PERIOD) = (From query In getDatabaseEntity.PERIODs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                             query.GAME_LINK_ID = selectedGame.ID).ToList
            If periodList IsNot Nothing Then
                numberOfPeriodTextBlock.Text = periodList.Count.ToString
                Dim currentPeriod As PERIOD = periodList.Find(Function(x)
                                                                  If x.STATUS = ProgressStatusEnum.inProgress Then
                                                                      Return True
                                                                  Else
                                                                      Return False
                                                                  End If
                                                              End Function)
                If currentPeriod IsNot Nothing Then
                    periodTextBlock.Text = currentPeriod.PERIOD_NAME + " (" + currentPeriod.PERIOD_NUMBER.ToString + ")"
                End If
            End If

            Dim firmList As List(Of GAME_FIRM) = (From query In getDatabaseEntity.GAME_FIRM Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                                query.GAME_LINK_ID = selectedGame.ID).ToList
            If firmList IsNot Nothing Then
                numberOfFirmsTextBlock.Text = firmList.Count.ToString
            End If

            Dim playerList As List(Of USER) = (From query In getDatabaseEntity.USER_USER_ROLE_GAME_FIRM_RELATIONSHIP Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                                                         query.GAME_LINK_ID = selectedGame.ID Select query.USER).ToList
            If playerList IsNot Nothing Then
                numberOfPlayersTextBlock.Text = playerList.Count.ToString
            End If
        End If
    End Sub

    Private Sub GameCardView_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseDown
        If extraInfo.Visibility = Visibility.Collapsed Then
            extraInfo.Visibility = Visibility.Visible
        Else
            extraInfo.Visibility = Visibility.Collapsed
        End If

        If gameSelectedDelegate IsNot Nothing AndAlso selectedGame IsNot Nothing Then
            gameSelectedDelegate(selectedGame)
        End If
    End Sub
End Class
