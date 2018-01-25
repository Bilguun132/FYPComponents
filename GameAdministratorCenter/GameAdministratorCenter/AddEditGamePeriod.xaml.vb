Imports GameAdministratorCenter.Contracts

Public Class AddEditGamePeriod
    Public Property period As PERIOD
    Public Property mode As Integer?

    Public Sub New(passedInPeriod As PERIOD, passedInMode As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        period = passedInPeriod
        mode = passedInMode
        loadPage()
    End Sub

    Private Sub loadPage()
        periodNameTextBox.Text = period.PERIOD_NAME
        If period.EXPECTED_START IsNot Nothing Then
            expectedStartDateEdit.EditValue = period.EXPECTED_START
            expectedStartDateEdit.UpdateLayout()
        End If
        If period.EXPECTED_END IsNot Nothing Then
            expectedEndDateEdit.EditValue = period.EXPECTED_END
            expectedEndDateEdit.UpdateLayout()
        End If
        simulationDurationSpinEdit.EditValue = period.SIMULATION_DURATION
    End Sub

    Public Function pullPeriodFromUI() As PERIOD
        period.PERIOD_NAME = periodNameTextBox.Text
        period.EXPECTED_START = expectedStartDateEdit.DateTime
        period.EXPECTED_END = expectedEndDateEdit.DateTime
        period.SIMULATION_DURATION = simulationDurationSpinEdit.EditValue
        Return period
    End Function
End Class
