Imports System.Windows
Imports GameAdministratorCenter.Contracts
Imports ContentManager
Imports System.Timers

Public Class ManageGamePLayUserControl
    Private selectedGameId As Integer?
    Private currentRunTime As TimeSpan
    Private currentPeriod As PERIOD
    Private periodList As List(Of PERIOD)
    Dim timer As Timer

    Public Sub New(passedInId As Integer?)
        InitializeComponent()
        selectedGameId = passedInId
    End Sub

    Private Sub ManageGamePLayUserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If selectedGameId IsNot Nothing Then
            loadPage()
        End If
    End Sub

    Private Sub loadPage()
        loadPeriodInformation()
        loadFirmList()
    End Sub

    Private Sub loadFirmList()
        Dim firmListUserControl As New FirmListUserControl(selectedGameId)
        firmListContainer.Content = firmListUserControl
    End Sub

    Private Sub loadPeriodInformation()
        If timer IsNot Nothing Then
            timer.Stop()
            timer = Nothing
        End If

        periodList = (From query In getDatabaseEntity.PERIODs Where query.GAME_LINK_ID = selectedGameId AndAlso query.PERIOD_NUMBER IsNot Nothing AndAlso
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
            currentPeriod = periodList.Find(Function(x)
                                                If x.START_TIME IsNot Nothing AndAlso x.END_TIME Is Nothing Then
                                                    Return True
                                                Else
                                                    Return False
                                                End If
                                            End Function)
            If currentPeriod Is Nothing Then
                currentPeriod = periodList.Find(Function(x)
                                                    If x.START_TIME Is Nothing AndAlso x.END_TIME Is Nothing Then
                                                        Return True
                                                    Else
                                                        Return False
                                                    End If
                                                End Function)
                If currentPeriod Is Nothing Then
                    currentPeriod = periodList.Last()
                End If
            End If

            If currentPeriod IsNot Nothing Then
                populatePeriod(currentPeriod)
            End If
        End If
    End Sub

    Private Sub populatePeriod(currentPeriod As PERIOD)
        emptyPeriodUIFields()

        currentPeriodNameTextblock.Text = currentPeriod.PERIOD_NAME
        currentPeriodNumberTextblock.Text = currentPeriod.PERIOD_NUMBER
        If currentPeriod.EXPECTED_START IsNot Nothing Then
            expectedStartDateTextBlock.Text = Date.Parse(currentPeriod.EXPECTED_START).ToString("dd/MM/yyyy HH:mm")
        End If

        If currentPeriod.EXPECTED_END IsNot Nothing Then
            expectedEndDateTextBlock.Text = Date.Parse(currentPeriod.EXPECTED_END).ToString("dd/MM/yyyy HH:mm")
        End If

        If currentPeriod.START_TIME Is Nothing Then
            currentPeriodStatusTextBlock.Text = "Not Started"
            currentPeriodStatusImage.Source = convertBitmapToBitmapSource(My.Resources.Orange_circle)
            startButton.IsEnabled = True
            stopButton.IsEnabled = False
        Else
            actualStartDateTextBlock.Text = Date.Parse(currentPeriod.START_TIME).ToString("dd/MM/yyyy HH:mm")
            If currentPeriod.END_TIME Is Nothing Then
                currentPeriodStatusTextBlock.Text = "In Progress"
                currentPeriodStatusImage.Source = convertBitmapToBitmapSource(My.Resources.Green_circle)
                stopButton.IsEnabled = True
                startButton.IsEnabled = False
                currentRunTime = Date.Now - currentPeriod.START_TIME
                progressBarEdit.Value = currentRunTime.TotalSeconds / (Date.Parse(currentPeriod.EXPECTED_END) - Date.Parse(currentPeriod.EXPECTED_START)).TotalSeconds * 100
                populateRunTime()
                startTimerToUpdateRunTime()
            Else
                actualEndDateTextBlock.Text = Date.Parse(currentPeriod.END_TIME).ToString("dd/MM/yyyy HH:mm")
                currentPeriodStatusTextBlock.Text = "Completed"
                currentPeriodStatusImage.Source = convertBitmapToBitmapSource(My.Resources.Blue_circle)
                currentRunTime = currentPeriod.END_TIME - currentPeriod.START_TIME
                progressBarEdit.Value = currentRunTime.TotalSeconds / (Date.Parse(currentPeriod.EXPECTED_END) - Date.Parse(currentPeriod.EXPECTED_START)).TotalSeconds * 100
                stopButton.IsEnabled = False
                startButton.IsEnabled = False
            End If
        End If


    End Sub

    Private Sub emptyPeriodUIFields()
        currentPeriodNameTextblock.Text = ""
        currentPeriodNumberTextblock.Text = ""
        currentPeriodStatusTextBlock.Text = ""
        currentPeriodStatusImage.Source = Nothing
        expectedStartDateTextBlock.Text = ""
        expectedEndDateTextBlock.Text = ""
        actualStartDateTextBlock.Text = ""
        actualEndDateTextBlock.Text = ""
        currentRunTimeTextblock.Text = ""
        currentSimulationTextblock.Text = ""
    End Sub

    Private Sub populateRunTime()
        currentRunTimeTextblock.Text = currentRunTime.ToString("hh\:mm\:ss")
        If currentPeriod.EXPECTED_START IsNot Nothing AndAlso currentPeriod.EXPECTED_END IsNot Nothing AndAlso currentPeriod.SIMULATION_DURATION IsNot Nothing Then
            Dim currentSimulationRate As Decimal = currentPeriod.SIMULATION_DURATION / (Date.Parse(currentPeriod.EXPECTED_END) - Date.Parse(currentPeriod.EXPECTED_START)).TotalDays
            Dim currentSimulationDays As TimeSpan = TimeSpan.FromDays(currentRunTime.TotalDays * currentSimulationRate)
            Dim currentMonth As Integer = Math.Floor(currentSimulationDays.TotalDays / 30)
            Dim currentDay As Integer = Math.Floor(currentSimulationDays.TotalDays - currentMonth * 30)
            Dim currentHour As Integer = Math.Floor((currentSimulationDays.TotalDays - currentMonth * 30 - currentDay) * 24)
            currentSimulationTextblock.Text = String.Format("{0} month(s) {1} day(s) {2} hours", currentMonth, currentDay, currentHour)
        End If
    End Sub

    Private Sub startTimerToUpdateRunTime()
        If timer IsNot Nothing Then
            timer.Stop()
            timer = Nothing
        End If
        timer = New Timer
        timer.Interval = 1000
        timer.AutoReset = True
        AddHandler timer.Elapsed, AddressOf updateRunTime
        timer.Start()
    End Sub

    Private Sub updateRunTime(sender As Object, e As ElapsedEventArgs)
        currentRunTime += TimeSpan.FromSeconds(1)
        Dispatcher.BeginInvoke(Sub()
                                   populateRunTime()
                                   progressBarEdit.Value += 1 / (Date.Parse(currentPeriod.EXPECTED_END) - Date.Parse(currentPeriod.EXPECTED_START)).TotalSeconds * 100
                               End Sub)
    End Sub

    Private Sub startButton_Click(sender As Object, e As RoutedEventArgs) Handles startButton.Click
        currentPeriod.START_TIME = Date.Now
        currentPeriod.STATUS = ProgressStatusEnum.inProgress
        progressBarEdit.Value = 0

        If currentPeriod.PERIOD_NUMBER = 1 Then
            currentPeriod.GAME.STATUS = ProgressStatusEnum.inProgress
        End If
        getDatabaseEntity.SaveChanges()
        loadPeriodInformation()
    End Sub

    Private Sub stopButton_Click(sender As Object, e As RoutedEventArgs) Handles stopButton.Click
        currentPeriod.STATUS = ProgressStatusEnum.completed
        If currentPeriod.PERIOD_NUMBER = periodList.Count Then
            currentPeriod.GAME.STATUS = ProgressStatusEnum.completed
        End If
        Dim marketInfo As MARKET_INFO = (From query In getDatabaseEntity.MARKET_INFO Where query.ID = 1).FirstOrDefault
        updateMarketInfo(marketInfo, currentPeriod.SIMULATION_DURATION / 365)
        updateProductionRevenue(currentPeriod.ID)
        currentPeriod.END_TIME = Date.Now
        getDatabaseEntity.SaveChanges()
        loadPeriodInformation()
    End Sub
End Class
